using Shield_Shot.Core.SceneFlow;
using Shield_Shot.GameplayCore.Network;
using Shield_Shot.GameplayCore.Network.Match;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shield_Shot.GameplayCore.Network.Pvp
{
    public sealed class PvpMatchEndPresenter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PvpMatchStateController _matchStateController;
        [SerializeField] private PvpLocalPlayerSideProvider _localPlayerSideProvider;

        [Header("Scene")]
        [SerializeField] private string _lobbySceneName = "02_Lobby";

        [Header("Auto Return")]
        [SerializeField] private bool _autoReturnToLobby = true;
        [SerializeField] private float _autoReturnDelaySeconds = 3f;

        private PvpMatchState _lastState = PvpMatchState.None;
        private bool _returnRequested;
        private bool _isReturningToLobby;
        private float _returnAtTime;

        private void Awake()
        {
            if (_matchStateController == null)
            {
                _matchStateController = FindFirstObjectByType<PvpMatchStateController>();
            }

            if (_localPlayerSideProvider == null)
            {
                _localPlayerSideProvider = FindFirstObjectByType<PvpLocalPlayerSideProvider>();
            }
        }

        private void Update()
        {
            if (_matchStateController == null)
            {
                _matchStateController = FindFirstObjectByType<PvpMatchStateController>();
                if (_matchStateController == null)
                {
                    return;
                }
            }

            if (_matchStateController.Object == null || !_matchStateController.Object.IsValid)
            {
                return;
            }

            PvpMatchState currentState = _matchStateController.CurrentState;
            if (_lastState != currentState)
            {
                _lastState = currentState;

                if (currentState == PvpMatchState.MatchEnded)
                {
                    OnMatchEnded();
                }
            }

            if (_returnRequested &&
                _autoReturnToLobby &&
                Time.unscaledTime >= _returnAtTime)
            {
                ReturnToLobby("MatchEnded");
            }
        }

        private void OnMatchEnded()
        {
            PlayerSide winnerSide = ResolveWinnerSide();
            bool isLocalWinner = IsLocalWinner(winnerSide);

            Debug.Log($"[PvpMatchEndPresenter] Match ended. Winner: {winnerSide}, LocalWinner: {isLocalWinner}");

            if (!_autoReturnToLobby)
            {
                return;
            }

            _returnRequested = true;
            _returnAtTime = Time.unscaledTime + _autoReturnDelaySeconds;
            Debug.Log($"[PvpMatchEndPresenter] Auto return scheduled: {_autoReturnDelaySeconds}s");
        }

        public void ReturnToLobbyFromButton()
        {
            ReturnToLobby("MatchEnded");
        }

        private async void ReturnToLobby(string reason)
        {
            if (_isReturningToLobby)
            {
                return;
            }

            _isReturningToLobby = true;
            _returnRequested = false;

            Debug.Log($"[PvpMatchEndPresenter] Returning to lobby. Reason: {reason}");

            if (NetworkMatchManager.Instance != null)
            {
                await NetworkMatchManager.Instance.ShutdownNetworkAsync();
            }
            else
            {
                Debug.LogWarning("[PvpMatchEndPresenter] NetworkMatchManager is missing.");
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
                Debug.LogWarning("[PvpMatchEndPresenter] SceneFlowManager is missing. Loading lobby directly.");
                SceneManager.LoadScene(_lobbySceneName);
            }
        }

        private PlayerSide ResolveWinnerSide()
        {
            if (_matchStateController.BottomScore >= _matchStateController.TargetScore)
            {
                return PlayerSide.Bottom;
            }

            if (_matchStateController.TopScore >= _matchStateController.TargetScore)
            {
                return PlayerSide.Top;
            }

            return PlayerSide.Bottom;
        }

        private bool IsLocalWinner(PlayerSide winnerSide)
        {
            if (_localPlayerSideProvider == null)
            {
                _localPlayerSideProvider = FindFirstObjectByType<PvpLocalPlayerSideProvider>();
            }

            return _localPlayerSideProvider != null &&
                   _localPlayerSideProvider.TryGetLocalSide(out PlayerSide localSide) &&
                   localSide == winnerSide;
        }
    }
}
