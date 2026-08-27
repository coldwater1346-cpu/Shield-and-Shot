using UnityEngine;
using UnityEngine.EventSystems; //  드래그 시스템 인터페이스를 쓰기 위해 필수!

namespace Shield_Shot.DataManagement.InventorySystem
{
    // IDragHandler 인터페이스를 상속받으면 유니티가 드래그를 자동으로 감지합니다.
    public class UIItemDragRotator : MonoBehaviour, IDragHandler
    {
        public void OnDrag(PointerEventData eventData)
        {
            if (Item3DPreviewManager.Instance == null) return;

            // 마우스/손가락이 이번 프레임에 좌우로 움직인 거리(delta.x)를 측정
            float mouseX = eventData.delta.x;

            // 측정된 값을 저 멀리 세트장에 있는 매니저에게 실시간으로 토스!
            Item3DPreviewManager.Instance.RotateModel(mouseX);
        }
    }
}