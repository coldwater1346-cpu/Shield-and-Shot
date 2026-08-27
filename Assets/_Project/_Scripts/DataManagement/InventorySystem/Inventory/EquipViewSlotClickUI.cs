using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Shield_Shot.DataManagement.InventorySystem
{
    public class EquipViewSlotClickUI : MonoBehaviour, IPointerClickHandler
    {
        // 인스펙터에서 MainWeapon, SubWeapon, Shield 중 하나를 마우스로 툭 고르면 끝!
        [SerializeField] private EquipSlotType _targetSlotType;

        private Item _targetItem;
        private InventoryManager _inventoryManager;
        private Action _onSelected;

        public void SetEquipTarget(Item item, InventoryManager inventoryManager, Action onSelected)
        {
            _targetItem = item;
            _inventoryManager = inventoryManager;
            _onSelected = onSelected;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_targetItem == null || _inventoryManager == null) return;

            Debug.Log($"장착 슬롯 {_targetSlotType}에 {_targetItem.ItemData.ItemName} 장착 시도");

            // 매니저에게 인덱스가 아닌 Enum 타입을 던집니다!
            _inventoryManager.EquipItem(_targetItem, _targetSlotType);

            _onSelected?.Invoke();
        }
    }
}
