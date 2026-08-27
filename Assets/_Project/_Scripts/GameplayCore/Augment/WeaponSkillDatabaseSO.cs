using System.Collections.Generic;
using Shield_Shot.GameplayCore.Weapon.Projectile;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Augment
{
    [CreateAssetMenu(fileName = "WeaponSkillDatabase", menuName = "ProjectileSystem/WeaponSkillDatabase")]
    public class WeaponSkillDatabaseSO : ScriptableObject
    {
        [System.Serializable]
        public struct Entry
        {
            public WeaponSkillType SkillType;
            public ActiveWeaponSkillSO SkillSO;
        }

        [Header("무기 SkillType(뽑기 결과)별 ActiveWeaponSkillSO 매핑")]
        public List<Entry> Entries;

        public ActiveWeaponSkillSO GetSkill(WeaponSkillType skillType)
        {
            if (skillType == WeaponSkillType.None || Entries == null) return null;

            foreach (var entry in Entries)
            {
                if (entry.SkillType == skillType)
                    return entry.SkillSO;
            }

            return null;
        }
    }
}