using Shield_Shot.NetworkCore;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 

namespace Shield_Shot.DataManagement.InventorySystem
{
    public class UpgradePanelUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InventoryManager _inventoryManager;
        [SerializeField] private UpgradeSlotListUI _slotListUI;
        [SerializeField] private UpgradeMaterialPopupUI _materialPopupUI;

        [Header("Selected Item View")]
        [SerializeField] private ViewSlotUI _targetViewSlotUI;

        
        [Header("Stat UI")]
        [SerializeField] private TMP_Text _statText; 

        [Header("Button")]
        [SerializeField] private Button _openMaterialPopupButton;
        [SerializeField] private Button _upgradeButton;

        [SerializeField] private AudioClip _upgradeSfx;
        [SerializeField, Range(0f, 1f)] private float _upgradeSfxVolume = 0.5f;

        private readonly List<Item> _upgradeItems = new List<Item>();
        private readonly List<Item> _selectedMaterials = new List<Item>();

        private Item _targetItem;
        private InventorySlotUI _targetSlotUI;

        private void Awake()
        {
            if (_inventoryManager == null)
                _inventoryManager = FindFirstObjectByType<InventoryManager>();

            if (_slotListUI == null)
                _slotListUI = FindFirstObjectByType<UpgradeSlotListUI>();

            if (_materialPopupUI == null)
                _materialPopupUI = FindFirstObjectByType<UpgradeMaterialPopupUI>();

            ClearSelectedViews();
        }

        private void OnEnable()
        {
            if (_inventoryManager != null)
                _inventoryManager.OnInventoryChanged += RefreshUI;

            if (_slotListUI != null)
                _slotListUI.OnClickSlot += OnClickSlot;

            if (_openMaterialPopupButton != null)
                _openMaterialPopupButton.onClick.AddListener(OnClickOpenMaterialPopupButton);

            if (_upgradeButton != null)
                _upgradeButton.onClick.AddListener(OnClickUpgradeButton);

            RefreshUI();
        }

        private void OnDisable()
        {
            if (_inventoryManager != null)
                _inventoryManager.OnInventoryChanged -= RefreshUI;

            if (_slotListUI != null)
                _slotListUI.OnClickSlot -= OnClickSlot;

            if (_openMaterialPopupButton != null)
                _openMaterialPopupButton.onClick.RemoveListener(OnClickOpenMaterialPopupButton);

            if (_upgradeButton != null)
                _upgradeButton.onClick.RemoveListener(OnClickUpgradeButton);
        }

        private void RefreshUI()
        {
            if (_inventoryManager == null || _slotListUI == null)
                return;

            // 기존 선택 아이템 기억
            Item previousTargetItem = _targetItem;

            // 기존 슬롯 하이라이트 참조만 초기화
            if (_targetSlotUI != null)
            {
                _targetSlotUI.SetHighlight(false);
                _targetSlotUI = null;
            }

            _upgradeItems.Clear();

            foreach (Item item in _inventoryManager.Items)
            {
                if (item == null)
                    continue;

                if (item.CanEnhance)
                    _upgradeItems.Add(item);
            }

            _slotListUI.DrawSlots(_upgradeItems);

            // 기존 선택 아이템이 여전히 인벤토리에 있고 강화 가능하면 유지
            if (previousTargetItem != null &&
                _upgradeItems.Contains(previousTargetItem))
            {
                _targetItem = previousTargetItem;

                if (_targetViewSlotUI != null)
                    _targetViewSlotUI.SetItem(_targetItem);

                _selectedMaterials.Clear();

                UpdateStatText();
            }
            else
            {
                ClearSelection();
            }
        }

        private void OnClickSlot(InventorySlotUI slotUI, Item item)
        {
            if (item == null)
                return;

            if (_targetSlotUI == slotUI)
            {
                ClearSelection();
                return;
            }

            SetTargetItem(slotUI, item);
        }

        private void SetTargetItem(InventorySlotUI slotUI, Item item)
        {
            if (_targetSlotUI != null)
                _targetSlotUI.SetHighlight(false);

            _targetSlotUI = slotUI;
            _targetItem = item;

            _targetSlotUI.SetHighlight(true);

            _selectedMaterials.Clear();

            if (_targetViewSlotUI != null)
                _targetViewSlotUI.SetItem(item);

            //  �������� ���� �������� �� ���� �ؽ�Ʈ ����
            UpdateStatText();
        }

        private void OnClickOpenMaterialPopupButton()
        {
            if (_targetItem == null)
            {
                Debug.LogWarning("���� ��ȭ�� �������� �����ؾ� �մϴ�.");
                return;
            }

            if (_materialPopupUI == null)
            {
                Debug.LogWarning("UpgradeMaterialPopupUI�� ������� �ʾҽ��ϴ�.");
                return;
            }

            _materialPopupUI.Open(
                _targetItem,
                _inventoryManager,
                _selectedMaterials,
                OnSelectedMaterialsChanged
            );
        }

        private void OnSelectedMaterialsChanged(List<Item> selectedMaterials)
        {
            _selectedMaterials.Clear();
            _selectedMaterials.AddRange(selectedMaterials);

            Debug.Log($"���õ� ��ȭ ��� ����: {_selectedMaterials.Count}");

            //  �˾����� ��� ���� ���°� �ٲ� ������ �ǽð����� ���� ���� ����
            UpdateStatText();
        }

        private void OnClickUpgradeButton()
        {
            if (_targetItem == null)
            {
                Debug.LogWarning("��ȭ ����� �����ؾ� �մϴ�.");
                return;
            }

            if (!_targetItem.CanEnhance)
            {
                Debug.LogWarning("�̹� �ִ� ��ȭ �����Դϴ�.");
                return;
            }

            if (_selectedMaterials.Count <= 0)
            {
                Debug.LogWarning("��ȭ ��Ḧ �����ؾ� �մϴ�.");
                return;
            }
            else
            {
                // ��ȭ ó�� 
            }
        }

        private void UpdateStatText()
        {
            if (_statText == null)
                return;

            if (_targetItem == null)
            {
                _statText.text = "";
                return;
            }

            int baseDamage = 0;
            string statName = "Enhance Level";

            // 무기인 경우 공격력 표시
            if (_targetItem.ItemData is WeaponItemData weaponData)
            {
                baseDamage = weaponData.BaseDamage;
                statName = "Damage";
            }

            // 현재 능력치
            int currentTotalDamage = baseDamage + _targetItem.EnhanceLevel;

            // 현재 능력치만 표시
            _statText.text = $"{statName} : {currentTotalDamage}";
        }

        private void ClearSelection()
        {
            ClearSelectedHighlights();
            ClearSelectedViews();

            _targetItem = null;
            _selectedMaterials.Clear();

            //  ���� ���� �� �ؽ�Ʈ�� �ʱ�ȭ
            if (_statText != null) _statText.text = "";
        }

        private void ClearSelectedHighlights()
        {
            if (_targetSlotUI != null)
                _targetSlotUI.SetHighlight(false);

            _targetSlotUI = null;
        }

        private void ClearSelectedViews()
        {
            if (_targetViewSlotUI != null)
                _targetViewSlotUI.Clear();
        }

        public void ClearPanelSelection()
        {
            ClearSelection();

            if (_materialPopupUI != null)
                _materialPopupUI.ClosePopup();
        }
    }
}