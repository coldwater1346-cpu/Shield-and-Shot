using UnityEngine;

[CreateAssetMenu(fileName = "NewShopItem", menuName = "Shop/Item")]
public class ShopItemData : ScriptableObject
{
    public string ItemName;
    public string Description;
    public Sprite ItemIcon;
    public int Price;
    public string CurrencyType;
}
