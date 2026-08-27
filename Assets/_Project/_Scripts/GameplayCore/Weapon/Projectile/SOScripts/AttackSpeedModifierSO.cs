using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Projectile
{
    public abstract class AttackSpeedModifierSO : ProjectileBehaviorSO
    {
        // 공격속도는 투사체에 직접 주입할 게 없으므로 항상 비워둔다.
        public override void InjectBehavior(ProjectileBase projectile, int currentLevel) { }

        /// <summary>기본 쿨타임(초)에 이 특성의 레벨을 적용한 최종 쿨타임(초)을 반환한다.</summary>
        public abstract float ApplyCooldown(float baseCooldown, int level);
    }
}