using Shield_Shot.UI.Core;
using UnityEngine;
using UnityEngine.UI;


namespace Shield_Shot.UI
{
    public class BottomMenuBarUI : MonoBehaviour
    {
        [Header("Menu Button")]
        [SerializeField] private Button _shopBtn;
        [SerializeField] private Button _gachaBtn;
        [SerializeField] private Button _storyBtn;
        [SerializeField] private Button _inventoryBtn;
        [SerializeField] private Button _arenaBtn;

        private MenuType _currentType = MenuType.NONE;

        void Start()
        {
            UIEventBus.OnMenuChanged += (type) => _currentType = type;

            _shopBtn.onClick.AddListener(() => ToggleMenu(MenuType.SHOP));
            _gachaBtn.onClick.AddListener(() => ToggleMenu(MenuType.GACHA));
            _storyBtn.onClick.AddListener(() => ToggleMenu(MenuType.STORY));
            _inventoryBtn.onClick.AddListener(() => ToggleMenu(MenuType.INVENTORY));
            _arenaBtn.onClick.AddListener(() => ToggleMenu(MenuType.ARENA));
        }

        private void ToggleMenu(MenuType type)
        {
            MenuType nextType = (_currentType == type) ? MenuType.NONE : type;
            UIEventBus.RaiseMenuChanged(nextType);
        }
    }
}

