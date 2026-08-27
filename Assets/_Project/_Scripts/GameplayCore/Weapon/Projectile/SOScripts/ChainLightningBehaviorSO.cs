using Shield_Shot.GameplayCore.Render;
using Shield_Shot.InputSystem.Data;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Projectile
{
    [CreateAssetMenu(menuName = "Shield Shot/Projectile/Behaviors/Chain Lightning", fileName = "ChainLightningBehaviorSO")]
    public class ChainLightningBehaviorSO : ProjectileBehaviorSO
    {
        [Header("Chain Lightning")]
        [SerializeField, Min(0)] private int chainCount = 3;
        [SerializeField, Min(0f)] private float searchRadius = 4f;
        [SerializeField, Range(0f, 1f)] private float chainDamageFalloff = 0.2f;
        [SerializeField, Min(0f)] private float chainDelay = 0.06f;
        [SerializeField] private LayerMask targetLayer;

        [Header("VFX")]
        [SerializeField] private VFXType hitVfxType = VFXType.Hit;
        [SerializeField, Min(0f)] private float vfxAutoReleaseTime = 1.5f;

        [Header("Sound")]
        [SerializeField] private AudioClip chainSfx;
        [SerializeField] private float _volume = 1f;

        public override bool CanInject(ProjectileBase projectile)
        {
            return projectile != null && projectile.SourceWeaponType == WeaponType.Bow;
        }

        public override void InjectBehavior(ProjectileBase projectile, int currentLevel)
        {
            if (!CanInject(projectile)) return;

            int level = Mathf.Max(1, currentLevel);
            int finalChainCount = chainCount + level - 1;

            projectile.AddHitBehavior(
                new ChainLightningHitBehavior(
                    finalChainCount,
                    searchRadius,
                    chainDamageFalloff,
                    targetLayer,
                    hitVfxType,
                    vfxAutoReleaseTime,
                    chainDelay,
                    chainSfx,
                    _volume),
                Priority
            );
        }
    }
}