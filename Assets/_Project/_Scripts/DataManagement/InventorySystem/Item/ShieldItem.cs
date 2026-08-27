using UnityEngine;

namespace Shield_Shot.DataManagement.InventorySystem
{

    public class ShieldItem : Item
    {


        public ShieldItemData ShieldData
        {
            get
            {
                return ItemData as ShieldItemData;
            }
        }

        //추상 클래스를 베이스로한 생성자 정의
        public ShieldItem(ShieldItemData data) : base(data)
        {

        }

        public ShieldItem(ShieldItemData data, string uniqueId, int enhanceLevel , ItemPropertyType property) : base(data, uniqueId, enhanceLevel, property) { }
    }
}