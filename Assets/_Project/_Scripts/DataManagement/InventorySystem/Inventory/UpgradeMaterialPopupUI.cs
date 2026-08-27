using Shield_Shot.DataManagement;
using Shield_Shot.DataManagement.DataParsing;
using Shield_Shot.DataManagement.InventorySystem;
using Shield_Shot.NetworkCore;
using Shield_Shot.UI;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shield_Shot.DataManagement.InventorySystem
{
    public class UpgradeMaterialPopupUI : MonoBehaviour
    {
        [SerializeField] private GameObject _listPopupPanel;

        [Header("Slot")]
        [SerializeField] private Transform _slotParent;
        [SerializeField] private InventorySlotUI _slotPrefab;

        [Header("Button")]
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _closeButton;

        [Header("Enhance Cost UI")]
        [SerializeField] private TMP_Text _requiredGoldText;

        [Header("Enhance Effects (Pre-placed UI)")]
        [SerializeField] private GameObject upgradeEffectObject; //  배치해둔 이펙트 오브젝트 

        [Header("사운드 클립")]
        [SerializeField] private AudioClip UpgradeClip; //  강화 효과음 

        private readonly List<InventorySlotUI> _createdSlots = new List<InventorySlotUI>();
        private readonly List<Item> _selectedMaterials = new List<Item>();

        private Item _targetItem;
        private InventoryManager _inventoryManager;
        private Action<List<Item>> _onConfirm;

        private void Awake()
        {
            _listPopupPanel.SetActive(false);
            _requiredGoldText.gameObject.SetActive(false);

        }

        private void OnEnable()
        {
            if (_confirmButton != null)
                _confirmButton.onClick.AddListener(OnClickUpgradeButton);

            if (_closeButton != null)
                _closeButton.onClick.AddListener(Close);
        }

        private void OnDisable()
        {
            if (_confirmButton != null)
                _confirmButton.onClick.RemoveListener(OnClickUpgradeButton);

            if (_closeButton != null)
                _closeButton.onClick.RemoveListener(Close);
        }

        public void Open(
            Item targetItem,
            InventoryManager inventoryManager,
            List<Item> alreadySelectedMaterials,
            Action<List<Item>> onConfirm)
        {
            _targetItem = targetItem;
            _inventoryManager = inventoryManager;
            _onConfirm = onConfirm;

            _selectedMaterials.Clear();

            if (alreadySelectedMaterials != null)
            {
                int selectableCount = GetSelectableMaterialCount();

                for (int i = 0; i < alreadySelectedMaterials.Count; i++)
                {
                    if (_selectedMaterials.Count >= selectableCount)
                        break;

                    _selectedMaterials.Add(alreadySelectedMaterials[i]);
                }
            }

            _listPopupPanel.SetActive(true);
            DrawMaterialSlots();
        }

        private int GetSelectableMaterialCount()
        {
            if (_targetItem == null)
                return 0;

            return Mathf.Max(0, _targetItem.ItemData.MaxEnhanceLevel - _targetItem.EnhanceLevel);
        }

        private void DrawMaterialSlots()
        {
            ClearSlots();

            if (_targetItem == null || _inventoryManager == null)
            {
                //  ����� ������ ��� �ؽ�Ʈ�� ��Ȱ��ȭ
                if (_requiredGoldText != null) _requiredGoldText.gameObject.SetActive(false);
                return;
            }

            foreach (Item item in _inventoryManager.Items)
            {
                if (item == null)
                    continue;

                if (item == _targetItem)
                    continue;

                if (item.ItemData == null || _targetItem.ItemData == null)
                    continue;

                // ���� ������ ����(ID)�� ���� ������ üũ
                if (item.ItemData.itemID != _targetItem.ItemData.itemID)
                    continue;

                if (item.IsEquipped)
                    continue; 

                
                InventorySlotUI slot = Instantiate(_slotPrefab, _slotParent);
                slot.gameObject.SetActive(true);

                slot.SetSlot(item, OnClickMaterialSlot);

                bool isSelected = _selectedMaterials.Contains(item);
                slot.SetHighlight(isSelected);

                _createdSlots.Add(slot);
            }

            
            if (_requiredGoldText != null)
            {
                bool hasMaterials = _createdSlots.Count > 0;
                _requiredGoldText.gameObject.SetActive(hasMaterials);

                
                if (hasMaterials)
                {
                    RefreshRequiredCostUI();
                }
            }
        }

        private void OnClickMaterialSlot(InventorySlotUI slotUI, Item item)
        {
            if (item == null)
                return;

            if (_selectedMaterials.Contains(item))
            {
                _selectedMaterials.Remove(item);
                RefreshRequiredCostUI();
                slotUI.SetHighlight(false);
                return;
            }

            int selectableCount = GetSelectableMaterialCount();

            if (_selectedMaterials.Count >= selectableCount)
            {
                Debug.LogWarning($"�� �̻� ������ �� �����ϴ�. ���� ���� ����: {selectableCount}");
                return;
            }

            _selectedMaterials.Add(item);
            RefreshRequiredCostUI();
            slotUI.SetHighlight(true);
        }

        private void OnClickUpgradeButton()
        {
            Debug.Log("��ȭ ��ư Ŭ����");

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
           
            ExecuteUpgrade();
        }

        private void ExecuteUpgrade()
        {
            int maxUseCount = GetSelectableMaterialCount();
            int materialCount = Mathf.Min(_selectedMaterials.Count, maxUseCount);

            int totalCostGold = 0;
            int tempLevel = _targetItem.EnhanceLevel;

            for (int i = 0; i < materialCount; i++)
            {
                if (!_targetItem.CanEnhance)
                    break;

                totalCostGold += ItemDataParsingManager.Instance.GetEnhanceCost(tempLevel);
                tempLevel++;
            }

            if (PlayerDataManager.Instance.gold < totalCostGold)
            {
                Debug.LogWarning($"��尡 �����Ͽ� ��ȭ�� ������ �� �����ϴ�. �ʿ�: {totalCostGold}, ����: {PlayerDataManager.Instance.gold}");
                return;
            }

            PlayerDataManager.Instance.gold -= totalCostGold;
            UIEventBus.RaiseCurrencyChanged();
            Debug.Log($"��ȭ ��� ���� �Ϸ�. �Ҹ� ���: {totalCostGold} | ���� ���: {PlayerDataManager.Instance.gold}");

            for (int i = 0; i < materialCount; i++)
            {
                if (!_targetItem.CanEnhance)
                    break;

                _targetItem.Enhance();
            }

            for (int i = 0; i < materialCount; i++)
            {
                _inventoryManager.RemoveItem(_selectedMaterials[i]);
            }

            Debug.Log($"��ȭ �Ϸ�. ����� ��� ����: {materialCount}");

            _onConfirm?.Invoke(new List<Item>(_selectedMaterials));

            _selectedMaterials.Clear();

            Close();

            // 강화 이펙트 출력
            PlayUpgradeEffect();
            // 강화 성공 사운드 재생
            if (UpgradeClip != null)
            {
                Shield_Shot.Audio.SoundManager.Instance.PlayUI(UpgradeClip,0.1f);
            }

            BackendGameData.Instance.GameDataUpdateAsync();
            Debug.Log("�ڳ� ���� �񵿱� ������ ȣ��");
        }

        private void Close()
        {
            ClearSlots();

            _targetItem = null;
            _inventoryManager = null;
            _onConfirm = null;

            
            if (_requiredGoldText != null) _requiredGoldText.gameObject.SetActive(false);

            _listPopupPanel.SetActive(false);
        }

        private void ClearSlots()
        {
            foreach (InventorySlotUI slot in _createdSlots)
            {
                if (slot != null)
                    Destroy(slot.gameObject);
            }

            _createdSlots.Clear();
        }

        public void RefreshRequiredCostUI()
        {
            if (_targetItem == null)
            {
                _requiredGoldText.text = "필요 골드 : 0";
                return;
            }

            int maxUseCount = GetSelectableMaterialCount();
            int materialCount = Mathf.Min(_selectedMaterials.Count, maxUseCount);

            int totalCostGold = 0;
            int tempLevel = _targetItem.EnhanceLevel;

            for (int i = 0; i < materialCount; i++)
            {
                totalCostGold += ItemDataParsingManager.Instance.GetEnhanceCost(tempLevel);
                tempLevel++;
            }

            if (materialCount == 0)
            {
                totalCostGold = ItemDataParsingManager.Instance.GetEnhanceCost(_targetItem.EnhanceLevel);
            }

            _requiredGoldText.text = $"필요 골드 : {totalCostGold.ToString("N0")}";

            if (PlayerDataManager.Instance.gold < totalCostGold)
            {
                _requiredGoldText.color = Color.red;
            }
            else
            {
                _requiredGoldText.color = Color.black;
            }
        }

        private void PlayUpgradeEffect()
        {
            if (upgradeEffectObject == null) return;

            upgradeEffectObject.SetActive(false);
            upgradeEffectObject.SetActive(true);

            // 코드로 파티클을 강제 재생
            ParticleSystem[] particles = upgradeEffectObject.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem ps in particles)
            {
                ps.Play();
            }

            CancelInvoke(nameof(DisableUpgradeEffect));
            Invoke(nameof(DisableUpgradeEffect), 3.0f);
        }

        private void DisableUpgradeEffect()
        {
            if (upgradeEffectObject != null)
            {
                upgradeEffectObject.SetActive(false);
            }
        }
        public void ClosePopup()
        {
            Close();
        }
    }
}