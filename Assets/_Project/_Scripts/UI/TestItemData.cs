using UnityEngine;



[CreateAssetMenu(fileName = "TestItemData", menuName = "Scriptable Objects/TestItemData")]
public class TestItemData : ScriptableObject
{
    public string _itemName;
    public ItemType _itemType;
    public Sprite _itemIcon;
}
