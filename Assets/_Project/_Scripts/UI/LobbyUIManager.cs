using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Shield_Shot.UI
{
    public class LobbyUIManager : MonoBehaviour
    {
        [System.Serializable]
        public struct MenuPanelMapping
        {
            public MenuType _menuType;
            public GameObject _panelObject;
            public Button _menuButton;
            public Color _activeColor;

            // 개별 메뉴에 속한 팝업들을 인스펙터에서 넣을 수 있도록 추가
            public List<GameObject> _popupObjects;
        }

        [SerializeField] private List<MenuPanelMapping> _menuPanels;
        [SerializeField] private Color _defaultColor = Color.white;

        private void OnEnable()
        {
            UIEventBus.OnMenuChanged += HandleMenuChanged;
        }
        private void OnDisable()
        {
            UIEventBus.OnMenuChanged -= HandleMenuChanged;
        }

        void Start()
        {
            UIEventBus.RaiseMenuChanged(MenuType.NONE);
        }

        private void HandleMenuChanged(MenuType newMenuType)
        {
            Debug.Log($"[LobbyUI] {newMenuType} 패널 활성화");

            foreach (var mapping in _menuPanels)
            {
                if (mapping._panelObject == null) continue;

                bool shouldActive = (mapping._menuType == newMenuType);
                mapping._panelObject.SetActive(shouldActive);

                if (mapping._menuButton != null)
                {
                    mapping._menuButton.image.color = shouldActive ? mapping._activeColor : _defaultColor;
                }
                // 핵심: 메인 메뉴가 닫힐 때 관련된 팝업들도 전부 닫기
                if (!shouldActive && mapping._popupObjects != null)
                {
                    foreach (var popup in mapping._popupObjects)
                    {
                        if (popup != null)
                        {
                            popup.SetActive(false); // 팝업 다 끄기
                        }
                    }
                }
            }
        }
    }

}