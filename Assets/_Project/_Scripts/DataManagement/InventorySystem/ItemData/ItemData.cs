using Shield_Shot.DataManagement.DataParsing;
using UnityEngine;


namespace Shield_Shot.DataManagement.InventorySystem
{

    [CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
    public class ItemData : ScriptableObject, ITableData
    {

        //아이템 공통 필드 

        [Header("Item Info")]
        public string itemID;
        public string ItemID => itemID;

        public string ItemName;
        public ItemType ItemType;
        public ItemGradeType ItemGradeType;
        public Sprite Icon;

        public string Description;


        //등급별 최대 강화 수치
        public int MaxEnhanceLevel
        {
            get
            {
                switch (ItemGradeType)
                {
                    case ItemGradeType.C:
                        return 10;

                    case ItemGradeType.UC:
                        return 15;

                    case ItemGradeType.Rare:
                        return 20;

                    case ItemGradeType.SR:
                        return 25;

                    case ItemGradeType.SSR:
                        return 30;

                    case ItemGradeType.UR:
                        return 35;

                    default:
                        return 10;
                }
            }
        }
        public void LinkData(ItemDataParsingManager manager)
        {
            // 현재는 연결할 데이터 없음
        }
    }
}