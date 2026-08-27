using UnityEngine;

namespace Shield_Shot.DataManagement.InventorySystem
{
    public class WeaponItem : Item
    {
        // 플레이어가 실제로 소유하는 무기 아이템

        // ItemData를 WeaponItemData 타입으로 변환
        public WeaponItemData WeaponData
        {
            get
            {
                return ItemData as WeaponItemData;
            }
        }

        public WeaponSkillType SkillType { get; protected set; } = WeaponSkillType.None;

        // 강화 레벨에 따른 아이템의 최종 데미지
        // 추후 특성, 버프, 증강 등의 수치가 추가
        public int FinalDamage
        {
            get
            {
                if (WeaponData == null)
                    return 0;

                return WeaponData.BaseDamage + EnhanceLevel;
            }
        }
        public float FinalFireRate
        {
            get
            {
                if (WeaponData == null)
                    return 0;

                return WeaponData.FireRate;
            }
        }

        public float FinalSpeed
        {
            get
            {
                if (WeaponData == null)
                    return 0;

                return WeaponData.BaseSpeed;
            }
        }

        public float FinalCriticalDamageMultiplier
        {
            get
            {
                if (WeaponData == null)
                    return 0;
                
                return WeaponData.CriticalDamageMultiplier;
            }
        }

        public float FinalMaxDamageMultiplier
        {
            get
            {
                if (WeaponData == null)
                    return 1f;

                return WeaponData.MaxDamageMultiplier;
            }
        }

        public float FinalMaxSpeedMultiplier
        {
            get
            {
                if (WeaponData == null)
                    return 1f;

                return WeaponData.MaxSpeedMultiplier;
            }
        }


        //추상 클래스를 베이스로한 생성자 정의
        public WeaponItem(WeaponItemData data) : base(data)
        {

        }
        public WeaponItem(WeaponItemData data, string uniqueId, int enhanceLevel, ItemPropertyType property) :base(data, uniqueId, enhanceLevel, property) { }

        // 무기 스킬(SkillType)까지 함께 지정하는 생성자. 뽑기/합성 결과를 바로 반영하고 싶을 때 사용.
        public WeaponItem(WeaponItemData data, string uniqueId, int enhanceLevel, ItemPropertyType property, WeaponSkillType skillType)
            : base(data, uniqueId, enhanceLevel, property)
        {
            SkillType = skillType;
        }

        public void SetSkillType(WeaponSkillType skillType)
        {
            SkillType = skillType;
        }
    }
}