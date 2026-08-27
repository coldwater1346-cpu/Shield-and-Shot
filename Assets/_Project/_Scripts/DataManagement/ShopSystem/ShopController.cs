using Shield_Shot.UI;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace Shield_Shot.DataManagement.ShopSystem
{
    public class ShopController : MonoBehaviour
    {
        [SerializeField] List<ShopItemData> _itemList;
        [SerializeField] GameObject _itemPrefab;
        [SerializeField] Transform _contentParent;

        [SerializeField] GameObject _purchaseDiaPanel;
        [SerializeField] GameObject _purchaseGoldPanel;

        [SerializeField] TextMeshProUGUI _confirmText1;
        [SerializeField] TextMeshProUGUI _confirmText2;

        private ShopItemData _selectedItem;

        private void Start()
        {
            GenerateShopItems();
        }

        private void GenerateShopItems()
        {
            foreach(var data in _itemList)
            {
                GameObject obj = Instantiate(_itemPrefab, _contentParent);
                obj.GetComponent<ShopItemSlot>().Setup(data, this);

            }
        }

        public void OpenPurchasePanel(ShopItemData data)
        {
            _selectedItem = data;
            _confirmText1.text = $"{data.ItemName}을(를) 구매하시겠습니까?\n(비용: {data.CurrencyType}{data.Price})";
            _confirmText2.text = $"{data.ItemName}을(를) 구매하시겠습니까?\n(비용: {data.CurrencyType}{data.Price})";
            if(data.ItemName == "Gold")
            {
                _purchaseGoldPanel.SetActive(true);
            }
            else
            {
                _purchaseDiaPanel.SetActive(true);
            }
        }

        public void OnConfirmPurchase()
        {
            _purchaseDiaPanel.SetActive(false);
            _purchaseGoldPanel.SetActive(false);

            // 서버 연동 로직(성공할때까지 출력 유지)
        }

        public void OnCanclePurchase()
        {
            _purchaseDiaPanel.SetActive(false);
            _purchaseGoldPanel.SetActive(false);
            _selectedItem = null;
        }
    }
}

