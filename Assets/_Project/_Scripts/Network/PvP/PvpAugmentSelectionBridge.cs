using Shield_Shot.GameplayCore.Network.Match;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Network.Pvp
{
    public sealed class PvpAugmentSelectionBridge : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PvpMatchStateController _matchStateController;
        [SerializeField] private PvpLocalPlayerSideProvider _localPlayerSideProvider;

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

        public void NotifySelectionCompleted()
        {
            if (_matchStateController == null)
            {
                _matchStateController = FindFirstObjectByType<PvpMatchStateController>();
            }

            if (_localPlayerSideProvider == null)
            {
                _localPlayerSideProvider = FindFirstObjectByType<PvpLocalPlayerSideProvider>();
            }

            if (_matchStateController == null)
            {
                Debug.LogWarning("[PvpAugmentSelectionBridge] MatchStateController is missing.");
                return;
            }

            if (_localPlayerSideProvider == null ||
                !_localPlayerSideProvider.TryGetLocalSide(out PlayerSide localSide))
            {
                Debug.LogWarning("[PvpAugmentSelectionBridge] Local player side is missing.");
                return;
            }

            Debug.Log($"[PvpAugmentSelectionBridge] Augment selection completed. Side: {localSide}");

            _matchStateController.NotifyLocalAugmentSelectionCompleted(localSide);
        }
    }
}