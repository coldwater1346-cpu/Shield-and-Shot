using System;
using UnityEngine;
using UnityEngine.UI;

namespace Shield_Shot.DataManagement.InventorySystem
{
    public class InventoryHelpUI : MonoBehaviour
    {
        [Header("Dependency")]
        [SerializeField] private InventoryTabUI _inventoryTabUI;

        [Header("UI Components")]
        [SerializeField] private Button _helpButton;
        [SerializeField] private GameObject _helpPopupPanel;
        [SerializeField] private ScrollRect _helpScrollRect;
        [SerializeField] private Button _closeHelpButton;

        [Header("Help Text Objects")]
        [SerializeField] private GameObject _equipHelpObject;
        [SerializeField] private GameObject _upgradeHelpObject;
        [SerializeField] private GameObject _combineHelpObject;

        private void OnEnable()
        {
            if (_helpButton != null)
                _helpButton.onClick.AddListener(OnClickHelpButton);

            if (_inventoryTabUI != null)
            {
                _inventoryTabUI.OnTabChanged += UpdateHelpView;
                UpdateHelpView(_inventoryTabUI.CurrentTab);
            }

            if (_closeHelpButton != null)
                _closeHelpButton.onClick.AddListener(Close);
        }

        private void OnDisable()
        {
            if (_helpButton != null)
                _helpButton.onClick.RemoveListener(OnClickHelpButton);

            if (_inventoryTabUI != null)
                _inventoryTabUI.OnTabChanged -= UpdateHelpView;

            if(_closeHelpButton != null)
                _closeHelpButton.onClick.RemoveListener(Close);
        }

        private void Awake()
        {
            if(_inventoryTabUI == null)
                _inventoryTabUI = FindFirstObjectByType<InventoryTabUI>();
        }


        // 선택된 탭에 맞춰 해당하는 텍스트 오브젝트 활성화

        public void UpdateHelpView(InventoryTabType tabType)
        {
            if (_equipHelpObject != null)
                _equipHelpObject.SetActive(tabType == InventoryTabType.Equip);

            if (_upgradeHelpObject != null)
                _upgradeHelpObject.SetActive(tabType == InventoryTabType.Upgrade);

            if (_combineHelpObject != null)
                _combineHelpObject.SetActive(tabType == InventoryTabType.Combine);

            // 탭이 바뀔 때 스크롤바 위치를 맨 위로 초기화
            ResetScrollPosition();
        }

        private void ResetScrollPosition()
        {
            if (_helpScrollRect != null)
            {
                // 1.0f 가 맨 위, 0.0f 가 맨 아래입니다.
                _helpScrollRect.verticalNormalizedPosition = 1.0f;
            }
        }

        private void OnClickHelpButton()
        {
            if (_helpPopupPanel != null)
            {
                _helpPopupPanel.SetActive(!_helpPopupPanel.activeSelf);

                // 팝업이 열릴 때도 스크롤 맨 위로 올려주기
                if (_helpPopupPanel.activeSelf)
                {
                    ResetScrollPosition();
                }
            }
        }

        private void Close()
        {
            if (_helpPopupPanel != null)
            {
                _helpPopupPanel.SetActive(false);
            }
        }
    }
}