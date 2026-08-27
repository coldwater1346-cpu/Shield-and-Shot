using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Projectile
{
    public abstract class ActiveWeaponSkillSO : ProjectileBehaviorSO
    {
        [Header("Active Skill Base")]
        [Tooltip("스킬 재사용 대기시간 (초). 실제 타이머 상태는 호출 측에서 관리한다.")]
        [SerializeField, Min(0f)] private float cooldownDuration = 10f;
        public float CooldownDuration => cooldownDuration;

        public override void InjectBehavior(ProjectileBase projectile, int currentLevel) { }

        public abstract void Activate(
            MonoBehaviour coroutineHost,
            IProjectileFireHandler fireHandler,
            Transform firePoint,
            Vector3 aimDirection,
            float chargeRatio,
            int level);
    }
}