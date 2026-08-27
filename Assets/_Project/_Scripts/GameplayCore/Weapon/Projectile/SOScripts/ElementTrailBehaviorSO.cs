using Shield_Shot.Audio;
using Shield_Shot.GameplayCore.Field;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Projectile
{
    public enum ElementSfxTiming
    {
        OnFire,
        OnHit,
        Both
    }

    [CreateAssetMenu(menuName = "Shield Shot/Projectile/Behaviors/Element Trail", fileName = "ElementTrailBehaviorSO")]
    public class ElementTrailBehaviorSO : ProjectileBehaviorSO
    {
        [Header("Element Trail")]
        [SerializeField] private ElementType element = ElementType.Fire;
        [SerializeField, Min(0f)] private float elementDuration = 2f;
        [SerializeField, Min(0f)] private float paintRadius = 0.6f;

        [Header("Projectile Visual")]
        [SerializeField] private Material projectileOverrideMaterial;
        [SerializeField] private bool replaceAllProjectileMaterialSlots = true;

        [Header("Element SFX")]
        [Tooltip("이 특성의 사운드. SO 자체가 이미 element 하나에 고정되어 있으므로 " +
                 "이 SO(예: Fire Trail 에셋)에는 화염 SFX, 다른 element용 에셋에는 그에 맞는 SFX를 각각 넣으면 된다.")]
        [SerializeField] private AudioClip elementSfx;
        [SerializeField, Range(0f, 1f)] private float elementSfxVolume = 1f;
        [Tooltip("OnFire = 발사되는 순간 1회 재생 / OnHit = 적중했을 때 재생 / Both = 둘 다")]
        [SerializeField] private ElementSfxTiming sfxTiming = ElementSfxTiming.OnHit;

        [Header("Wind Field Boost")]
        [SerializeField] private bool applyWindFieldBoost;
        [SerializeField, Min(0f)] private float baseBoostDuration = 0.6f;
        [SerializeField, Min(0f)] private float boostDurationPerLevel = 0.12f;
        [SerializeField, Min(1f)] private float baseSpeedMultiplier = 1.25f;
        [SerializeField, Min(0f)] private float speedMultiplierPerLevel = 0.05f;
        [SerializeField, Min(0f)] private float baseKnockbackSpeed = 8f;
        [SerializeField, Min(0f)] private float knockbackSpeedPerLevel = 1.5f;
        [SerializeField, Min(0f)] private float knockbackDuration = 0.15f;

        [Header("Ice Freeze Hit")]
        [SerializeField] private bool applyFreezeOnHit;
        [SerializeField, Min(0f)] private float baseFreezeDuration = 1.5f;
        [SerializeField, Min(0f)] private float freezeDurationPerLevel = 0.2f;

        public override void InjectBehavior(ProjectileBase projectile, int currentLevel)
        {
            if (projectile == null)
            {
                return;
            }

            int level = Mathf.Max(1, currentLevel);
            float finalRadius = paintRadius + 0.1f * (level - 1);

            if (projectileOverrideMaterial != null)
            {
                projectile.ApplyMaterialOverride(projectileOverrideMaterial, replaceAllProjectileMaterialSlots);
                projectile.AddMovementBehavior(
                    new ProjectileMaterialOverrideBehavior(
                        projectileOverrideMaterial,
                        replaceAllProjectileMaterialSlots
                    ),
                    Priority
                );
            }

            projectile.AddMovementBehavior(
                new FieldPaintMovementBehavior(element, level, elementDuration, finalRadius),
                Priority
            );

            if (elementSfx != null)
            {
                // 발사되는 순간(InjectBehavior가 곧 발사 시점) 1회 재생
                if (sfxTiming == ElementSfxTiming.OnFire || sfxTiming == ElementSfxTiming.Both)
                {
                    if (SoundManager.Instance != null)
                        SoundManager.Instance.PlaySFX(elementSfx, elementSfxVolume);
                }

                // 적중 시 재생 (히트 비헤이비어로 등록해서 실제로 맞았을 때만 울리게 함)
                if (sfxTiming == ElementSfxTiming.OnHit || sfxTiming == ElementSfxTiming.Both)
                {
                    projectile.AddHitBehavior(new ElementSfxHitBehavior(elementSfx, elementSfxVolume), Priority);
                }
            }

            if (applyWindFieldBoost)
            {
                WindFieldProjectileBoostBehavior boostBehavior = new WindFieldProjectileBoostBehavior(
                    level,
                    baseBoostDuration,
                    boostDurationPerLevel,
                    baseSpeedMultiplier,
                    speedMultiplierPerLevel
                );

                projectile.AddMovementBehavior(boostBehavior, Priority);
                projectile.AddHitBehavior(
                    new WindBoostKnockbackHitBehavior(
                        boostBehavior,
                        level,
                        baseKnockbackSpeed,
                        knockbackSpeedPerLevel,
                        knockbackDuration
                    ),
                    Priority
                );
            }

            if (applyFreezeOnHit)
            {
                float freezeDuration = baseFreezeDuration + freezeDurationPerLevel * (level - 1);
                projectile.AddHitBehavior(new FreezeHitBehavior(freezeDuration), Priority);
            }
        }

        private class ElementSfxHitBehavior : IHitBehavior
        {
            private readonly AudioClip _clip;
            private readonly float _volume;

            public ElementSfxHitBehavior(AudioClip clip, float volume)
            {
                _clip = clip;
                _volume = volume;
            }

            public void OnHit(ProjectileBase projectile, Collider targetInfo)
            {
                if (_clip == null || SoundManager.Instance == null) return;
                SoundManager.Instance.PlaySFX(_clip, _volume);
            }
        }
    }
}