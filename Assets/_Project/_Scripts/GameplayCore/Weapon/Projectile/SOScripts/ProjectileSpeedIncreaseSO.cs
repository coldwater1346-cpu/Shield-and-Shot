using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Projectile
{
    [CreateAssetMenu(menuName = "Shield Shot/Projectile/Behaviors/Projectile Speed Increase", fileName = "ProjectileSpeedIncreaseSO")]
    public class ProjectileSpeedIncreaseSO : ProjectileBehaviorSO
    {
        [Header("Projectile Speed Increase (투사체 속도 증가)")]
        [Tooltip("1레벨 기준 속도 증가율 (%)")]
        [SerializeField, Range(0f, 200f)] private float basePercent = 10f;

        [Tooltip("레벨 1당 추가되는 속도 증가율 (%)")]
        [SerializeField, Range(0f, 100f)] private float percentPerLevel = 5f;

        public override void InjectBehavior(ProjectileBase projectile, int currentLevel)
        {
            if (projectile == null) return;

            int level = Mathf.Max(1, currentLevel);
            float increasePercent = basePercent + (level - 1) * percentPerLevel;
            float multiplier = 1f + increasePercent / 100f;

            projectile.BaseSpeed *= multiplier;
            projectile.Velocity *= multiplier;
        }
    }
}