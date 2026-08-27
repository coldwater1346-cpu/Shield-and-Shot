using System.Collections.Generic;
using Shield_Shot.GameplayCore.Weapon.Projectile;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Augment
{
    [CreateAssetMenu(fileName = "AugmentDatabase", menuName = "ProjectileSystem/AugmentDatabase")]
    public class AugmentDatabaseSO : ScriptableObject
    {
        [Header("모든 증강(특성) SO 에셋 목록")]
        public List<ProjectileBehaviorSO> AllBehaviors;
    }
}