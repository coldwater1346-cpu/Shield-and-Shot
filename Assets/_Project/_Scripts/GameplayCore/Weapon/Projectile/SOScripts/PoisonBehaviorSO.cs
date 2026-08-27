using UnityEngine;
using Shield_Shot.GameplayCore.Render;

namespace Shield_Shot.GameplayCore.Weapon.Projectile
{
    [CreateAssetMenu(menuName = "Shield Shot/Projectile/Behaviors/Poison", fileName = "PoisonBehaviorSO")]
    public class PoisonBehaviorSO : ProjectileBehaviorSO
    {
        [Header("Poison DOT")]
        [SerializeField, Min(0f)] private float duration = 4f;
        [SerializeField, Min(0.01f)] private float tickInterval = 0.5f;

        [Header("Poison VFX")]
        [SerializeField] private bool showDamagePopup = true;
        [SerializeField] private VFXType tickVfxType = VFXType.Hit;
        [SerializeField, Min(0f)] private float vfxAutoReleaseTime = 1.5f;

        [Tooltip("기본 화살 데미지 대비 독 총 피해 비율입니다. 0.8이면 기본 데미지의 80%를 지속 피해로 줍니다.")]
        [SerializeField, Min(0f)] private float totalDamageRatio = 0.8f;

        [Tooltip("틱당 최소 보장 데미지. 데미지가 너무 낮아도 최소 이 값 이상은 들어갑니다.")]
        [SerializeField, Min(0f)] private float minDamagePerTick = 2f;

        public override void InjectBehavior(ProjectileBase projectile, int currentLevel)
        {
            if (projectile == null)
            {
                return;
            }

            int level = Mathf.Max(1, currentLevel);

            float totalDamage = projectile.ProjectileDamage * totalDamageRatio;
            totalDamage += projectile.ProjectileDamage * 0.2f * (level - 1);

            int tickCount = Mathf.Max(1, Mathf.CeilToInt(duration / tickInterval));
            float damagePerTick = Mathf.Max(minDamagePerTick, totalDamage / tickCount);

            projectile.AddHitBehavior(
                new PoisonHitBehavior(
                    duration,
                    tickInterval,
                    damagePerTick,
                    showDamagePopup,
                    tickVfxType,
                    vfxAutoReleaseTime
                ),
                Priority
            );
        }
    }
}