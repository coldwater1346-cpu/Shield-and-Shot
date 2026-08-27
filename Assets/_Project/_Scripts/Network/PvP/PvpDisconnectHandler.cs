using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using Shield_Shot.Core.SceneFlow;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shield_Shot.GameplayCore.Network.Pvp
{
    public sealed class PvpDisconnectHandler : MonoBehaviour, INetworkRunnerCallbacks
    {
        [Header("Scene")]
        [SerializeField] private string _lobbySceneName = "02_Lobby";

        private NetworkRunner _runner;
        private bool _isReturningToLobby;

        private void Awake()
        {
            _runner = FindFirstObjectByType<NetworkRunner>();
        }

        private void OnEnable()
        {
            RegisterCallbacks();
        }

        private void OnDisable()
        {
            UnregisterCallbacks();
        }

        private void RegisterCallbacks()
        {
            if (_runner == null)
            {
                _runner = FindFirstObjectByType<NetworkRunner>();
            }

            if (_runner == null)
            {
                Debug.LogWarning("[PvpDisconnectHandler] NetworkRunner is missing.");
                return;
            }

            _runner.AddCallbacks(this);
            Debug.Log("[PvpDisconnectHandler] Runner callbacks registered.");
        }

        private void UnregisterCallbacks()
        {
            if (_runner == null)
            {
                return;
            }

            _runner.RemoveCallbacks(this);
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            if (_isReturningToLobby)
            {
                return;
            }

            if (runner == null || player == runner.LocalPlayer)
            {
                return;
            }

            Debug.LogWarning($"[PvpDisconnectHandler] Opponent left: {player}");

            ReturnToLobby("OpponentLeft");
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            if (_isReturningToLobby)
            {
                return;
            }

            Debug.LogWarning($"[PvpDisconnectHandler] Runner shutdown: {shutdownReason}");

            ReturnToLobby($"Shutdown:{shutdownReason}");
        }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
            if (_isReturningToLobby)
            {
                return;
            }

            Debug.LogWarning($"[PvpDisconnectHandler] Disconnected from server: {reason}");

            ReturnToLobby($"Disconnected:{reason}");
        }

        private async void ReturnToLobby(string reason)
        {
            if (_isReturningToLobby)
            {
                return;
            }

            _isReturningToLobby = true;

            if (_runner != null && _runner.IsRunning)
            {
                await _runner.Shutdown();
            }

            SceneTransitionData data = new SceneTransitionData(
                SceneType.InGame,
                SceneType.Lobby,
                SceneTransitionReason.ReturnToLobby
            );

            data.Set("PvpReturnReason", reason);

            if (SceneFlowManager.Instance != null)
            {
                SceneFlowManager.Instance.LoadScene(_lobbySceneName, data);
            }
            else
            {
                SceneManager.LoadScene(_lobbySceneName);
            }
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
        public void OnInput(NetworkRunner runner, NetworkInput input) { }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ArraySegment<byte> data) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    }
}