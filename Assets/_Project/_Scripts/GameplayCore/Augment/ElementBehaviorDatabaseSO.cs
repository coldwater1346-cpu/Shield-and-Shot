using System.Collections.Generic;
using Shield_Shot.GameplayCore.Weapon.Projectile;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Augment
{
    [CreateAssetMenu(fileName = "ElementBehaviorDatabase", menuName = "ProjectileSystem/ElementBehaviorDatabase")]
    public class ElementBehaviorDatabaseSO : ScriptableObject
    {
        [System.Serializable]
        public struct Entry
        {
            public ItemPropertyType Property;
            public ProjectileBehaviorSO BehaviorSO;
        }

        [Header("무기 Property(Fire/Ice/Lightning/Wind)별 ProjectileBehaviorSO 매핑")]
        public List<Entry> Entries;

        public ProjectileBehaviorSO GetBehavior(ItemPropertyType property)
        {
            if (property == ItemPropertyType.None || Entries == null) return null;

            foreach (var entry in Entries)
            {
                if (entry.Property == property)
                    return entry.BehaviorSO;
            }

            return null;
        }
    }
}