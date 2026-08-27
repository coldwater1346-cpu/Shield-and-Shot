using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using Shield_Shot.GameplayCore.Network.Match;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shield_Shot.GameplayCore.Network
{
    public sealed class NetworkMatchManager : PersistentSingleton<NetworkMatchManager>, INetworkRunnerCallbacks
    {
        private const int MaxPvpPlayers = 2;

        [Header("Network Core Prefab")]
        [SerializeField] private NetworkRunner _runnerPrefab;

        [Header("Quick Match")]
        [SerializeField] private string _gameSceneName = "90_TestSJ";

        private NetworkRunner _currentRunner;
        private bool _isStarting;
        private bool _isMatched;
        private bool _isLoadingGameScene;
        private bool _isQuitting;

        public event Action MatchmakingStarted;
        public event Action<MatchContext> QuickMatchSucceeded;
        public event Action<string> QuickMatchFailed;
        public event Action QuickMatchCanceled;
        public event Action MatchEnded;

        public NetworkRunner CurrentRunner => _currentRunner;
        public MatchContext CurrentMatchContext { get; private set; }
        public bool IsStarting => _isStarting;
        public bool IsMatched => _isMatched;
        public bool IsInMatch => _currentRunner != null && _currentRunner.IsRunning;

        public bool TryEnsureCurrentMatchContext(out MatchContext matchContext)
        {
            if (CurrentMatchContext != null)
            {
                matchContext = CurrentMatchContext;
                return true;
            }

            if (_currentRunner == null || GetActivePlayerCount(_currentRunner) < MaxPvpPlayers)
            {
                matchContext = null;
                return false;
            }

            CurrentMatchContext = BuildLocalMatchContext(_currentRunner);
            MatchContextStore.Set(CurrentMatchContext);

            matchContext = CurrentMatchContext;
            return true;
        }

        public void ShutdownNetwork()
        {
            _ = ShutdownNetworkAsync();
        }

        public async Task ShutdownNetworkAsync()
        {
            if (_currentRunner == null)
            {
                CleanupRunnerReferenceOnly();
                return;
            }

            NetworkRunner runner = _currentRunner;
            _currentRunner = null;

            runner.RemoveCallbacks(this);
            await runner.Shutdown();

            CleanupRunnerReferenceOnly();
        }

        public async void StartQuickMatch()
        {
            if (_isStarting)
            {
                Debug.Log("[NetworkMatchManager] Quick match is already starting.");
                return;
            }

            if (IsInMatch)
            {
                Debug.Log("[NetworkMatchManager] Runner is already in a match.");
                return;
            }

            if (_runnerPrefab == null)
            {
                NotifyQuickMatchFailed("NetworkRunner prefab is not assigned.");
                return;
            }

            if (FindSceneBuildIndex(_gameSceneName) < 0)
            {
                NotifyQuickMatchFailed($"Game scene '{_gameSceneName}' is not in Build Settings.");
                return;
            }

            _isStarting = true;
            _isMatched = false;
            _isLoadingGameScene = false;

            MatchmakingStarted?.Invoke();

            _currentRunner = Instantiate(_runnerPrefab);
            _currentRunner.name = "NetworkRunner_QuickMatch";
            _currentRunner.ProvideInput = true;
            _currentRunner.AddCallbacks(this);
            DontDestroyOnLoad(_currentRunner.gameObject);

            EnsureRunnerComponents(
                _currentRunner,
                out INetworkSceneManager sceneManager,
                out INetworkObjectProvider objectProvider);

            Debug.Log("[NetworkMatchManager] Starting quick match session.");

            StartGameResult result = await _currentRunner.StartGame(new StartGameArgs
            {
                GameMode = GameMode.AutoHostOrClient,
                PlayerCount = MaxPvpPlayers,
                SceneManager = sceneManager,
                ObjectProvider = objectProvider,
                MatchmakingMode = Fusion.Photon.Realtime.MatchmakingMode.FillRoom,
                IsOpen = true,
                IsVisible = true
            });

            _isStarting = false;

            if (!result.Ok)
            {
                string errorMessage = $"Quick match failed. Reason: {result.ShutdownReason}, Message: {result.ErrorMessage}";
                Debug.LogError($"[NetworkMatchManager] {errorMessage}");

                CleanupRunner();
                NotifyQuickMatchFailed(errorMessage);
                return;
            }

            Debug.Log("[NetworkMatchManager] Quick match session started. Waiting for opponent.");
        }

        public async void CancelQuickMatch()
        {
            if (_currentRunner == null)
            {
                NotifyQuickMatchCanceled();
                return;
            }

            NetworkRunner runner = _currentRunner;
            CleanupRunnerReferenceOnly();

            await runner.Shutdown();

            NotifyQuickMatchCanceled();
        }

        public async void LeaveMatch()
        {
            if (_currentRunner == null)
            {
                return;
            }

            NetworkRunner runner = _currentRunner;
            CleanupRunnerReferenceOnly();

            await runner.Shutdown();

            MatchEnded?.Invoke();
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log($"[NetworkMatchManager] Player joined: {player}");

            if (_isMatched)
            {
                return;
            }

            int playerCount = GetActivePlayerCount(runner);
            Debug.Log($"[NetworkMatchManager] Waiting players: {playerCount}/{MaxPvpPlayers}");

            if (playerCount < MaxPvpPlayers)
            {
                return;
            }

            MatchContext matchContext = BuildLocalMatchContext(runner);

            CurrentMatchContext = matchContext;
            MatchContextStore.Set(matchContext);

            _isMatched = true;
            QuickMatchSucceeded?.Invoke(matchContext);

            if (runner.IsServer)
            {
                LoadPvpScene(runner);
            }
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log($"[NetworkMatchManager] Player left: {player}");

            if (_isMatched)
            {
                _isMatched = false;
                MatchEnded?.Invoke();
            }
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            Debug.Log($"[NetworkMatchManager] Runner shutdown: {shutdownReason}");

            if (runner == _currentRunner)
            {
                CleanupRunnerReferenceOnly();
            }
        }

        public void OnConnectedToServer(NetworkRunner runner)
        {
            Debug.Log("[NetworkMatchManager] Connected to server.");
        }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
            Debug.LogWarning($"[NetworkMatchManager] Disconnected from server: {reason}");

            CleanupRunnerReferenceOnly();
            NotifyQuickMatchFailed($"Disconnected from server: {reason}");
        }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
            Debug.LogError($"[NetworkMatchManager] Connection failed: {reason}");

            CleanupRunner();
            NotifyQuickMatchFailed($"Connection failed: {reason}");
        }

        public void OnSceneLoadStart(NetworkRunner runner)
        {
            Debug.Log("[NetworkMatchManager] PvP scene load started.");
        }

        public void OnSceneLoadDone(NetworkRunner runner)
        {
            Debug.Log("[NetworkMatchManager] PvP scene load done.");
            _isLoadingGameScene = false;
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {
        }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
        {
        }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {
        }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
        {
        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
        }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
        }

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
        }

        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
        {
        }

        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
        {
        }

        private void OnApplicationQuit()
        {
            _isQuitting = true;
            ShutdownNetwork();
        }

        protected override void OnDestroy()
        {
            if (_isQuitting)
            {
                ShutdownNetwork();
            }

            base.OnDestroy();
        }

        private void LoadPvpScene(NetworkRunner runner)
        {
            if (_isLoadingGameScene)
            {
                return;
            }

            int buildIndex = FindSceneBuildIndex(_gameSceneName);
            if (buildIndex < 0)
            {
                NotifyQuickMatchFailed($"Game scene '{_gameSceneName}' is not in Build Settings.");
                return;
            }

            _isLoadingGameScene = true;

            SceneRef sceneRef = SceneRef.FromIndex(buildIndex);

            Debug.Log($"[NetworkMatchManager] Loading PvP scene: {_gameSceneName}");

            runner.LoadScene(
                sceneRef,
                LoadSceneMode.Single,
                LocalPhysicsMode.None,
                setActiveOnLoad: true);
        }

        private MatchContext BuildLocalMatchContext(NetworkRunner runner)
        {
            List<PlayerRef> players = GetSortedActivePlayers(runner);
            List<MatchPlayerInfo> playerInfos = new();

            for (int i = 0; i < players.Count; i++)
            {
                PlayerRef playerRef = players[i];
                PlayerSide side = i == 0 ? PlayerSide.Bottom : PlayerSide.Top;

                playerInfos.Add(new MatchPlayerInfo(
                    userId: BuildTemporaryUserId(playerRef),
                    nickname: BuildTemporaryNickname(playerRef),
                    playerRef: playerRef,
                    side: side));
            }

            return new MatchContext(
                matchId: Guid.NewGuid().ToString("N"),
                sessionName: runner.SessionInfo != null ? runner.SessionInfo.Name : string.Empty,
                sceneName: _gameSceneName,
                matchMode: MatchMode.PvpQuick,
                players: playerInfos);
        }

        private static List<PlayerRef> GetSortedActivePlayers(NetworkRunner runner)
        {
            List<PlayerRef> players = new();

            foreach (PlayerRef player in runner.ActivePlayers)
            {
                if (player.IsRealPlayer)
                {
                    players.Add(player);
                }
            }

            players.Sort((a, b) => a.PlayerId.CompareTo(b.PlayerId));
            return players;
        }

        private static int GetActivePlayerCount(NetworkRunner runner)
        {
            int count = 0;

            foreach (PlayerRef player in runner.ActivePlayers)
            {
                if (player.IsRealPlayer)
                {
                    count++;
                }
            }

            return count;
        }

        private static string BuildTemporaryUserId(PlayerRef playerRef)
        {
            return $"player-{playerRef.PlayerId}";
        }

        private static string BuildTemporaryNickname(PlayerRef playerRef)
        {
            return $"Player {playerRef.PlayerId}";
        }

        private static int FindSceneBuildIndex(string sceneNameOrPath)
        {
            if (string.IsNullOrWhiteSpace(sceneNameOrPath))
            {
                return SceneManager.GetActiveScene().buildIndex;
            }

            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);
                string fileName = System.IO.Path.GetFileNameWithoutExtension(path);

                if (string.Equals(path, sceneNameOrPath, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(fileName, sceneNameOrPath, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }

        private static void EnsureRunnerComponents(
            NetworkRunner runner,
            out INetworkSceneManager sceneManager,
            out INetworkObjectProvider objectProvider)
        {
            sceneManager = runner.GetComponent<INetworkSceneManager>();
            if (sceneManager == null)
            {
                sceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>();
            }

            objectProvider = runner.GetComponent<INetworkObjectProvider>();
            if (objectProvider == null)
            {
                objectProvider = runner.gameObject.AddComponent<NetworkObjectProviderDefault>();
            }
        }

        private void NotifyQuickMatchFailed(string message)
        {
            _isStarting = false;
            _isMatched = false;
            _isLoadingGameScene = false;

            QuickMatchFailed?.Invoke(message);
        }

        private void NotifyQuickMatchCanceled()
        {
            _isStarting = false;
            _isMatched = false;
            _isLoadingGameScene = false;

            QuickMatchCanceled?.Invoke();
        }

        private void CleanupRunner()
        {
            if (_currentRunner != null)
            {
                _currentRunner.RemoveCallbacks(this);
                Destroy(_currentRunner.gameObject);
            }

            CleanupRunnerReferenceOnly();
        }

        private void CleanupRunnerReferenceOnly()
        {
            _currentRunner = null;
            CurrentMatchContext = null;
            MatchContextStore.Clear();
            _isStarting = false;
            _isMatched = false;
            _isLoadingGameScene = false;
        }

    }
}
