using Shield_Shot.GameplayCore.Augment;
using Shield_Shot.GameplayCore.Weapon.Projectile;
using UnityEngine;

namespace Shield_Shot.GameplayCore.DebugTools
{
    public class GameplayDebugController : MonoBehaviour
    {
        [Header("Debug Toggle")]
        [SerializeField] private bool _enableDebugKeys = true;

        [Header("Augment Debug")]
        [SerializeField] private KeyCode _augmentPopupKey = KeyCode.Alpha1;
        [SerializeField] private AugmentPopupUI _augmentPopupUI;

        [Header("Chain Lightning Debug")]
        [SerializeField] private KeyCode _chainLightningKey = KeyCode.Alpha2;
        [SerializeField] private PlayerStatus _playerStatus;
        [SerializeField] private ProjectileBehaviorSO _chainLightningBehavior;
        [SerializeField, Min(1)] private int _chainLightningLevel = 1;

        [Header("Poison Debug")]
        [SerializeField] private KeyCode _poisonKey = KeyCode.Alpha3;
        [SerializeField] private ProjectileBehaviorSO _poisonBehavior;
        [SerializeField, Min(1)] private int _poisonLevel = 1;

        private void Awake()
        {
            if (_augmentPopupUI == null)
            {
                _augmentPopupUI = FindAnyObjectByType<AugmentPopupUI>(FindObjectsInactive.Include);
            }

            if (_playerStatus == null)
            {
                LocalPlayerStatusContext.TryGet(out _playerStatus);
            }
        }

        private void Update()
        {
            if (!_enableDebugKeys)
            {
                return;
            }

            if (Input.GetKeyDown(_augmentPopupKey) || Input.GetKeyDown(KeyCode.Keypad1))
            {
                OpenAugmentPopup();
            }

            if (Input.GetKeyDown(_chainLightningKey) || Input.GetKeyDown(KeyCode.Keypad2))
            {
                ReserveChainLightning();
            }

            if (Input.GetKeyDown(_poisonKey) || Input.GetKeyDown(KeyCode.Keypad3))
            {
                ReservePoison();
            }
        }
        private void ReservePoison()
        {
            if (_playerStatus == null)
            {
                LocalPlayerStatusContext.TryGet(out _playerStatus);
            }

            if (_playerStatus == null)
            {
                Debug.LogWarning("[GameplayDebugController] PlayerStatus is missing.");
                return;
            }

            if (_poisonBehavior == null)
            {
                Debug.LogWarning("[GameplayDebugController] PoisonBehaviorSO is missing.");
                return;
            }

            _playerStatus.ReserveNextShotBehavior(_poisonBehavior, _poisonLevel);
            Debug.Log("[GameplayDebugController] Poison reserved for next shot.");
        }

        private void OpenAugmentPopup()
        {
            if (_augmentPopupUI == null)
            {
                Debug.LogWarning("[GameplayDebugController] AugmentPopupUI is missing.");
                return;
            }

            _augmentPopupUI.OpenPopup();
            Debug.Log("[GameplayDebugController] Augment popup debug opened.");
        }

        private void ReserveChainLightning()
        {
            if (_playerStatus == null)
            {
                LocalPlayerStatusContext.TryGet(out _playerStatus);
            }

            if (_playerStatus == null)
            {
                Debug.LogWarning("[GameplayDebugController] PlayerStatus is missing.");
                return;
            }

            if (_chainLightningBehavior == null)
            {
                Debug.LogWarning("[GameplayDebugController] ChainLightningBehaviorSO is missing.");
                return;
            }

            _playerStatus.ReserveNextShotBehavior(_chainLightningBehavior, _chainLightningLevel);
            Debug.Log("[GameplayDebugController] Chain lightning reserved for next shot.");
        }
    }
}
