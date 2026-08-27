using UnityEngine;

namespace Shield_Shot.DataManagement.InventorySystem
{
    public abstract class Item
    {

        // 플레이어가 실제로 소유하는 아이템의 부모 클래스
        // ItemData(원본 데이터)를 참조
        // 강화 수치, 특성, 고유 ID 등 개별 상태를 관리

        //인벤토리에 들어가는건 하위 클래스라 추상 클래스로 만듬
        public ItemData ItemData { get; private set; }

        public string UniqueID { get; private set; }

        // 현재 강화 레벨
        public int EnhanceLevel { get; protected set; }

        public EquipSlotType CurrentSlotType { get; set; } = EquipSlotType.None;
        public bool IsEquipped => CurrentSlotType != EquipSlotType.None; // 장착 여부 판별 

        // 1. 특성 필드 및 프로퍼티 추가
        public ItemPropertyType Property { get; protected set; } = ItemPropertyType.None;

        //  특성 유무 판별 
        public bool HasProperty => Property != ItemPropertyType.None;

        public int SlotIndex { get; set; }

        //  강화 가능 여부(최대 강화전까지)
        public bool CanEnhance
        {
            get
            {
                return EnhanceLevel < ItemData.MaxEnhanceLevel;
            }
        }
        public void Enhance()
        {
            if (!CanEnhance)
            {
                Debug.Log("최대 강화 레벨입니다.");
                return;
            }

            EnhanceLevel++;
        }
        // 아이템이 처음 생성될 때 (기본적으로 특성 없이 생성되거나 필요시 주입 가능)
        protected Item(ItemData itemData, ItemPropertyType property = ItemPropertyType.None)
        {
            ItemData = itemData;
            UniqueID = System.Guid.NewGuid().ToString();
            EnhanceLevel = 0;
            Property = property; // 합성/소환 성공 시 주입된 특성 저장
        }

        // 기존 생성자 밑 불러오기(Load) 전용 생성자 (서버 저장용 데이터 복구)
        protected Item(ItemData itemData, string uniqueId, int enhanceLevel, ItemPropertyType property)
        {
            ItemData = itemData;
            UniqueID = uniqueId;
            EnhanceLevel = enhanceLevel;
            Property = property; // 저장되어 있던 특성 
        }

    }
}