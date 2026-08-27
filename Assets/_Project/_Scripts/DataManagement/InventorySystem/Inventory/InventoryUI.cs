using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Shield_Shot.DataManagement.InventorySystem
{
    public class InventoryUI : MonoBehaviour
    {
        private const int _defaultSlotCount = 30;
        private const int _columns = 5;

        [Header("References")]
        [SerializeField] private InventoryManager _inventoryManager;
        [SerializeField] private ItemInfoPanelUI _itemInfoPanelUI;

        private InventorySlotUI _selectedSlot;
        private Item _selectedItem;


        [Header("UI Elements")]
        [SerializeField] private InventorySlotUI _slotPrefab;
        [SerializeField] private Transform _slotsParent;

        [Header("Sort Buttons")]
        [SerializeField] private Button _sortByGradeButton;
        [SerializeField] private Button _sortByTypeButton;

        private readonly List<InventorySlotUI> _slots = new List<InventorySlotUI>();

        private void Awake()
        {
            if (_inventoryManager == null)
                _inventoryManager = FindFirstObjectByType<InventoryManager>();

            if (_itemInfoPanelUI == null)
                _itemInfoPanelUI = FindFirstObjectByType<ItemInfoPanelUI>();
        }

        private void OnEnable()
        {
            if (_inventoryManager != null)
                _inventoryManager.OnInventoryChanged += UpdateUI;

            // 버튼 이벤트 리스너 등록
            if (_sortByGradeButton != null)
                _sortByGradeButton.onClick.AddListener(OnClickSortByGrade);

            if (_sortByTypeButton != null)
                _sortByTypeButton.onClick.AddListener(OnClickSortByType);
        }

        private void OnDisable()
        {
            if (_inventoryManager != null)
                _inventoryManager.OnInventoryChanged -= UpdateUI;

            if (_sortByGradeButton != null)
                _sortByGradeButton.onClick.RemoveListener(OnClickSortByGrade);

            if (_sortByTypeButton != null)
                _sortByTypeButton.onClick.RemoveListener(OnClickSortByType);
        }

        private void Start()
        {
            UpdateUI();

       
        }

        private void UpdateUI()
        {
            int itemCount = _inventoryManager.Items.Count;

            int requiredSlotCount = CalculateRequiredSlotCount(itemCount);

            CreateExtraSlots(requiredSlotCount);
            UpdateSlots();
        }
        private int CalculateRequiredSlotCount(int itemCount)
        {
            int slotCount = _defaultSlotCount;

            while (slotCount < itemCount)
            {
                slotCount += _columns;
            }

            return slotCount;
        }
        private void CreateExtraSlots(int targetSlotCount)
        {
            while (_slots.Count < targetSlotCount)
            {
                InventorySlotUI slot = Instantiate(_slotPrefab, _slotsParent);
                _slots.Add(slot);
            }
        }
        private void UpdateSlots()
        {
            IReadOnlyList<Item> items = _inventoryManager.Items;

            for (int i = 0; i < _slots.Count; i++)
            {
                Item item = null;

                if (i < items.Count)
                {
                    item = items[i];
                }

                _slots[i].SetSlot(item, OnClickSlot);

            }
        }
        private void OnClickSlot(InventorySlotUI slot, Item item)
        {
            if (_selectedSlot != null)
            {
                _selectedSlot.SetHighlight(false);
            }

            _selectedSlot = slot;
            _selectedItem = item;

            _selectedSlot.SetHighlight(true);
            _itemInfoPanelUI.Show(_selectedItem);

            Debug.Log($"선택한 아이템 : {_selectedItem.ItemData.ItemName} / {_selectedItem.UniqueID}");
        }

        // 등급순 정렬 버튼
        private void OnClickSortByGrade()
        {
            if (_inventoryManager != null)
            {
                _inventoryManager.SortByGrade();
            }
        }

        // 종류별 정렬 버튼
        private void OnClickSortByType()
        {
            if (_inventoryManager != null)
            {
                _inventoryManager.SortByType();
            }
        }

    }
    
}