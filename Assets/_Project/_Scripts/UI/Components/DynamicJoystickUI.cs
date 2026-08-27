using Shield_Shot.InputSystem;
using Shield_Shot.InputSystem.Data;
using UnityEngine;
using Shield_Shot.GameplayCore.Augment;
using UnityEngine.UI;

namespace Shield_Shot.UI.Components
{
    /// <summary>
    /// 왼쪽(방패) 존에서 첫 터치가 발생한 위치에 조이스틱 배경을 띄우고,
    /// 드래그 오프셋만큼 노브를 움직여 보여주는 순수 시각 컴포넌트.
    /// 실제 방패 회전 로직(ShieldOrbitController)과는 무관하게 같은 GestureAnalyzer 이벤트를 구독한다.
    /// </summary>
    public class DynamicJoystickUI : MonoBehaviour
    {
        [Header("Input Source")]
        [Tooltip("이 조이스틱이 표시할 입력 존 (공격/방어)")]
        [SerializeField] private InputZone _targetZone = InputZone.Defend;

        [Tooltip("비어있으면 씬에서 _targetZone과 일치하는 GestureAnalyzer를 자동으로 찾는다.")]
        [SerializeField] private GestureAnalyzer _targetAnalyzer;

        [Header("References")]
        [SerializeField] private RectTransform _parentRect;   // 배경/노브가 배치될 캔버스(또는 그 하위) RectTransform
        [SerializeField] private RectTransform _background;   // 조이스틱 배경(원형) 이미지
        [SerializeField] private RectTransform _knob;          // 조이스틱 노브(스틱) 이미지
        [SerializeField] private Camera _uiCamera;              // ScreenSpaceOverlay면 비워둔다

        [Header("Feel Settings")]
        [Tooltip("ShieldOrbitController의 maxJoystickRadius와 같은 값으로 맞춰준다.")]
        [SerializeField] private float _maxRadius = 150f;

        private void Awake()
        {
            if (_targetAnalyzer == null)
            {
                GestureAnalyzer[] all = FindObjectsByType<GestureAnalyzer>(FindObjectsSortMode.None);
                foreach (var analyzer in all)
                {
                    if (analyzer.Zone == _targetZone)
                    {
                        _targetAnalyzer = analyzer;
                        break;
                    }
                }
            }

            DisableRaycastBlocking(_background);
            DisableRaycastBlocking(_knob);

            Hide();
        }

        private void OnEnable()
        {
            if (_targetAnalyzer == null) return;

            _targetAnalyzer.OnInputBegan += HandleInputBegan;
            _targetAnalyzer.OnInputStay += HandleInputStay;
            _targetAnalyzer.OnInputUp += HandleInputReleased;
            _targetAnalyzer.OnInputCanceled += HandleInputReleased;
        }

        private void OnDisable()
        {
            if (_targetAnalyzer == null) return;

            _targetAnalyzer.OnInputBegan -= HandleInputBegan;
            _targetAnalyzer.OnInputStay -= HandleInputStay;
            _targetAnalyzer.OnInputUp -= HandleInputReleased;
            _targetAnalyzer.OnInputCanceled -= HandleInputReleased;
        }

        private void HandleInputBegan(InputContext ctx)
        {
            if (AugmentPopupUI.IsOpen) return;

            if (_parentRect == null || _background == null) return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _parentRect, ctx.startPosition, _uiCamera, out Vector2 localPoint))
            {
                _background.anchoredPosition = localPoint;
            }

            if (_knob != null)
                _knob.anchoredPosition = Vector2.zero;

            Show();
        }

        private void HandleInputStay(InputContext ctx)
        {
            if (AugmentPopupUI.IsOpen)
            {
                Hide();
                return;
            }

            if (_knob == null) return;

            _knob.anchoredPosition = Vector2.ClampMagnitude(ctx.dragVector, _maxRadius);
        }

        private void HandleInputReleased(InputContext ctx)
        {
            Hide();

            if (_knob != null)
                _knob.anchoredPosition = Vector2.zero;
        }

        private void Show()
        {
            if (_background != null) _background.gameObject.SetActive(true);
        }

        private void Hide()
        {
            if (_background != null) _background.gameObject.SetActive(false);
        }

        private static void DisableRaycastBlocking(RectTransform target)
        {
            if (target == null) return;

            Graphic[] graphics = target.GetComponentsInChildren<Graphic>(true);
            foreach (Graphic graphic in graphics)
            {
                graphic.raycastTarget = false;
            }
        }
    }
}