using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shield_Shot.DataManagement.ShopSystem
{
    public class ShopItemSlot : MonoBehaviour
    {
        [SerializeField] Image _itemImage;
        [SerializeField] TextMeshProUGUI _nameText;
        [SerializeField] TextMeshProUGUI _priceText;

        private ShopItemData _itemData;
        private ShopController _shopController;

        public void Setup(ShopItemData data, ShopController manager)
        {
            _itemData = data;
            _shopController = manager;

            _itemImage.sprite = data.ItemIcon;
            _nameText.text = data.Description;
            //_priceText.text = data.Price.ToString();
        }

        public void OnClickItem()
        {
            _shopController.OpenPurchasePanel(_itemData);
        }
    }
}

