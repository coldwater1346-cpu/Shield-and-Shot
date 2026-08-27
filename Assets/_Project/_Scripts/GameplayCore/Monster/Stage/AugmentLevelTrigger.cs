using Shield_Shot.GameplayCore.Augment;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Progression
{
    /// 레벨업 시 증강 팝업을 연다. 여러 번 오르면 순차 처리(큐).
    public class AugmentLevelTrigger : MonoBehaviour
    {
        [SerializeField] private AugmentPopupUI _augmentPopup;

        private int _pending;
        private bool _open;

        private void Start()   // Awake 아님: LevelSystem.Awake 이후 보장
        {
            if (PlayerLevelSystem.Instance != null)
                PlayerLevelSystem.Instance.LeveledUp += OnLeveledUp;
        }
        private void OnDestroy()
        {
            if (PlayerLevelSystem.Instance != null)
                PlayerLevelSystem.Instance.LeveledUp -= OnLeveledUp;
        }

        private void OnLeveledUp(int level) { _pending++; TryOpenNext(); }

        private void TryOpenNext()
        {
            if (_open || _pending <= 0 || _augmentPopup == null) return;
            _open = true;
            Time.timeScale = 0f;
            _augmentPopup.OnAugmentSelectionCompleted += OnSelectionDone;
            _augmentPopup.OpenPopup();   // 팝업이 timeScale=0로 멈춤

        }

        private void OnSelectionDone()
        {
            _augmentPopup.OnAugmentSelectionCompleted -= OnSelectionDone;
            _open = false;
            _pending--;
            if (_pending > 0) TryOpenNext();
            else Time.timeScale = 1f;
        }
    }
}