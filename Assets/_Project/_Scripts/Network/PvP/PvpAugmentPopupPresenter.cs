using Shield_Shot.GameplayCore.Augment;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Network.Pvp
{
    public sealed class PvpAugmentPopupPresenter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PvpMatchStateController _matchStateController;
        [SerializeField] private AugmentPopupUI _augmentPopupUI;

        private PvpMatchState _lastState = PvpMatchState.None;

        private void Awake()
        {
            if (_matchStateController == null)
            {
                _matchStateController = FindFirstObjectByType<PvpMatchStateController>();
            }

            if (_augmentPopupUI == null)
            {
                _augmentPopupUI = FindFirstObjectByType<AugmentPopupUI>(FindObjectsInactive.Include);
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
            if (_lastState == currentState)
            {
                return;
            }

            _lastState = currentState;

            if (currentState == PvpMatchState.AugmentSelection)
            {
                OpenAugmentPopup();
            }
        }

        private void OpenAugmentPopup()
        {
            if (_augmentPopupUI == null)
            {
                _augmentPopupUI = FindFirstObjectByType<AugmentPopupUI>(FindObjectsInactive.Include);
            }

            if (_augmentPopupUI == null)
            {
                Debug.LogWarning("[PvpAugmentPopupPresenter] AugmentPopupUI is missing.");
                return;
            }

            Debug.Log("[PvpAugmentPopupPresenter] Open augment popup.");

            _augmentPopupUI.OpenPopup();
        }
    }
}
