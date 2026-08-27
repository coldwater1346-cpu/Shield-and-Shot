using UnityEngine;

namespace Shield_Shot.GameplayCore.Augment
{
    public class AugmentDebugTrigger : MonoBehaviour
    {
        [Header("연결할 팝업 UI 컴포넌트")]
        [SerializeField] private AugmentPopupUI augmentPopupUI;

        [Header("테스트 단축키 세팅")]
        [SerializeField] private KeyCode triggerKey = KeyCode.Alpha1; // 기본값: 숫자 1키

        private void Awake()
        {
            if (augmentPopupUI == null)
            {
                augmentPopupUI = FindAnyObjectByType<AugmentPopupUI>(FindObjectsInactive.Include);
            }
        }

        private void Update()
        {
            // 인게임에서 지정된 단축키(1번)를 누르는 순간 실행
            if (Input.GetKeyDown(triggerKey) || Input.GetKeyDown(KeyCode.Keypad1))
            {
                Debug.Log("1");
;                if (augmentPopupUI != null)
                {
                    Debug.Log($"<color=cyan>🛠️ [Debug] {triggerKey} 키 입력 감지 - 증강 팝업을 강제 작동합니다.</color>");
                    augmentPopupUI.OpenPopup();
                }
                else
                {
                    Debug.LogWarning("[AugmentDebugTrigger] 연결된 AugmentPopupUI가 인스펙터에 없습니다!");
                }
            }
        }
    }
}
