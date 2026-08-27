using UnityEngine;
using UnityEngine.UI;

namespace Shield_Shot.UI
{
    public class ShopPanelUI : MonoBehaviour
    {
        [SerializeField] private Button _closeBtn;
        [SerializeField] private Button _oneChanceBtn;
        [SerializeField] private Button _tenChanceBtn;

        private void Awake()
        {
            _closeBtn.onClick.AddListener(CloseSettingPanel);
            //_oneChanceBtn.onClick.AddListener();
            //_tenChanceBtn.onClick.AddListener();
        }

        private void CloseSettingPanel()
        {
            UIEventBus.RaiseMenuChanged(MenuType.NONE);
        }
    }
}

