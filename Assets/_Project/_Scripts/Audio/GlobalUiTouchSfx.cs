using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Shield_Shot.Audio
{
    public class GlobalUiTouchSfx : MonoBehaviour
    {
        [Header("공용 UI 터치음")]
        [SerializeField] private AudioClip touchSfxClip;
        [SerializeField, Range(0f, 1f)] private float volume = 1f;

        private readonly List<RaycastResult> _raycastResults = new List<RaycastResult>();

        private void Update()
        {
            if (Pointer.current == null) return;

            // 이번 프레임에 눌림(터치 시작/마우스 클릭)이 발생했는지
            if (!Pointer.current.press.wasPressedThisFrame) return;

            GameObject hitObject = GetTopUIObjectUnderPointer();
            if (hitObject == null) return; // UI 위가 아니면 아무 것도 하지 않는다.

            // 이 오브젝트(또는 부모)에 전용 클릭 사운드가 있으면, 공용 터치음은 양보한다.
            if (hitObject.GetComponentInParent<UIButtonClickSfx>() != null) return;

            if (touchSfxClip != null && SoundManager.Instance != null)
                SoundManager.Instance.PlayUI(touchSfxClip, volume);
        }

        private GameObject GetTopUIObjectUnderPointer()
        {
            if (EventSystem.current == null) return null;

            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Pointer.current.position.ReadValue()
            };

            _raycastResults.Clear();
            EventSystem.current.RaycastAll(pointerData, _raycastResults);

            return _raycastResults.Count > 0 ? _raycastResults[0].gameObject : null;
        }
    }
}