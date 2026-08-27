using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Projectile
{
    public abstract class CriticalDamageModifierSO : ProjectileBehaviorSO
    {
        public override void InjectBehavior(ProjectileBase projectile, int currentLevel) { }

        /// <summary>기본 크리티컬 배율에 이 특성의 레벨을 적용한 최종 배율을 반환한다.</summary>
        public abstract float ApplyCriticalMultiplier(float baseCriticalMultiplier, int level);
    }
}