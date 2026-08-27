using System;
using UnityEngine;
using UnityEngine.UI;

namespace Shield_Shot.DataManagement.InventorySystem
{
    public class InventoryTabUI : MonoBehaviour
    {
        [Header("Tab Button")]
        [SerializeField] private Button _equipTabButton;
        [SerializeField] private Button _upgradeTabButton;
        [SerializeField] private Button _combineTabButton;

        [Header("Tab Panels")]
        [SerializeField] private GameObject _equipPanel;
        [SerializeField] private GameObject _upgradePanel;
        [SerializeField] private GameObject _combinePanel;

        [Header("Tab Controllers")]
        [SerializeField] private EquipPanelUI _equipPanelUI;
        [SerializeField] private UpgradePanelUI _upgradePanelUI;
        [SerializeField] private CombinePanelUI _combinePanelUI;

        [Header("Child PopUI")]
        [SerializeField] private GameObject _upgradeMaterialPopupUI;
        [SerializeField] private GameObject _itemInfoPanelUI;
        // 항상 활성화된 루트의 ItemInfoPanelUI 스크립트
        [SerializeField]
        private ItemInfoPanelUI _itemInfoPanelUIController;

        // 탭 변경  이벤트
        public event Action<InventoryTabType> OnTabChanged;

        public InventoryTabType CurrentTab { get; private set; }

        private bool _isInitialized;

        private void OnEnable()
        {
            if (_equipTabButton != null)
                _equipTabButton.onClick.AddListener(OnClickEquipTab);

            if (_upgradeTabButton != null)
                _upgradeTabButton.onClick.AddListener(OnClickUpgradeTab);

            if (_combineTabButton != null)
                _combineTabButton.onClick.AddListener(OnClickCombineTab);

            // 인벤토리를 처음 열면 장착 탭으로 시작
            ChangeTab(InventoryTabType.Equip);
        }

        private void Awake()
        {
            if(_itemInfoPanelUIController==null)
                _itemInfoPanelUIController=FindFirstObjectByType<ItemInfoPanelUI>();
        }

        private void OnDisable()
        {
            if (_equipTabButton != null)
                _equipTabButton.onClick.RemoveListener(OnClickEquipTab);

            if (_upgradeTabButton != null)
                _upgradeTabButton.onClick.RemoveListener(OnClickUpgradeTab);

            if (_combineTabButton != null)
                _combineTabButton.onClick.RemoveListener(OnClickCombineTab);

            // 인벤토리 전체가 닫힐 때도 현재 선택 상태 초기화
            ClearCurrentTabSelection();

            CloseChildPopups();

            _isInitialized = false;
        }

        private void OnClickEquipTab()
        {
            ChangeTab(InventoryTabType.Equip);
        }

        private void OnClickUpgradeTab()
        {
            ChangeTab(InventoryTabType.Upgrade);
        }

        private void OnClickCombineTab()
        {
            ChangeTab(InventoryTabType.Combine);
        }

        private void ChangeTab(InventoryTabType tabType)
        {
            // 같은 탭 버튼을 다시 눌렀다면 전환하지 않음
            if (_isInitialized && CurrentTab == tabType)
                return;

            // 기존에 열려 있던 탭의 선택 상태 초기화
            if (_isInitialized)
                ClearCurrentTabSelection();

            CurrentTab = tabType;
            _isInitialized = true;

            // 메인 탭 패널 활성화/비활성화
            if (_equipPanel != null)
                _equipPanel.SetActive(tabType == InventoryTabType.Equip);

            if (_upgradePanel != null)
                _upgradePanel.SetActive(tabType == InventoryTabType.Upgrade);

            if (_combinePanel != null)
                _combinePanel.SetActive(tabType == InventoryTabType.Combine);

            // 탭을 전환하면 하위 팝업 닫기
            CloseChildPopups();

            // 탭이 변경 알림
            OnTabChanged?.Invoke(CurrentTab);

            Debug.Log($"현재 인벤토리 탭: {CurrentTab}");
        }

        private void ClearCurrentTabSelection()
        {
            switch (CurrentTab)
            {
           
                case InventoryTabType.Upgrade:
                    if (_upgradePanelUI != null)
                        _upgradePanelUI.ClearPanelSelection();
                    break;

                case InventoryTabType.Combine:
                    if (_combinePanelUI != null)
                        _combinePanelUI.ClearPanelSelection();
                    break;
            }
        }

        private void CloseChildPopups()
        {
            if (_upgradeMaterialPopupUI != null)
                _upgradeMaterialPopupUI.SetActive(false);

            if (_itemInfoPanelUI != null)
                _itemInfoPanelUI.SetActive(false);

            // 외부에 남아 있는 장착 슬롯 선택 UI 끄기
            if (_itemInfoPanelUIController != null)
                _itemInfoPanelUIController.HideEquipSlotHighlights();
        }
    }
}