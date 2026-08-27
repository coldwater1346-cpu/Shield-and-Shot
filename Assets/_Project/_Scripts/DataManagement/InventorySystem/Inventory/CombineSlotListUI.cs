using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shield_Shot.DataManagement.InventorySystem
{
    public class CombineSlotListUI : MonoBehaviour
    {
        [Header("Slot")]
        [SerializeField] private Transform _slotParent;
        [SerializeField] private InventorySlotUI _slotPrefab;

        private readonly List<InventorySlotUI> _createdSlots = new List<InventorySlotUI>();

        public event Action<InventorySlotUI, Item> OnClickSlot;

        public void DrawSlots(List<Item> items)
        {
            ClearSlots();

            foreach (Item item in items)
            {
                InventorySlotUI slot = Instantiate(_slotPrefab, _slotParent);
                slot.SetSlot(item, HandleClickSlot);

                _createdSlots.Add(slot);
            }
        }

        public void ClearSlots()
        {
            foreach (InventorySlotUI slot in _createdSlots)
            {
                if (slot != null)
                    Destroy(slot.gameObject);
            }

            _createdSlots.Clear();
        }

        private void HandleClickSlot(InventorySlotUI slotUI, Item item)
        {
            OnClickSlot?.Invoke(slotUI, item);
        }
    }
}