using Shield_Shot.GameplayCore.Weapon.Projectile;
using UnityEngine;

[CreateAssetMenu(menuName = "Projectile/Behavior/DefaultDamage")]
public class DefaultDamageSO : ProjectileBehaviorSO
{
    [Header("Sound")]
    [SerializeField] private AudioClip hitSfx;
    [SerializeField] private float _hitVolume = 1f;

    public override void InjectBehavior(ProjectileBase projectile, int currentLevel)
    {
        var statCalc = projectile.Launcher.StatCalculator;
        float baseDamage = statCalc.Calculate(0f).Damage;
        float levelBonus = (currentLevel - 1) * 5f;
        float leveledDamage = baseDamage + levelBonus;

        float chargeMultiplier = Mathf.Lerp(1f, statCalc.MaxDamageMultiplier, Mathf.Clamp01(projectile.ChargeRatio));
        float chargedDamage = leveledDamage * chargeMultiplier;

        // 크리티컬 배율 적용 (크리티컬일 때만)
        //  - 무기 기본 크리티컬 배율(statCalc.CriticalMultiplier)에
        //    CriticalDamageModifierSO 계열 증강이 있으면 그 보너스를 더 얹는다.
        bool isCritical = projectile.IsCritical;
        float criticalMultiplier = GetFinalCriticalMultiplier(statCalc.CriticalMultiplier);
        float finalDamage = isCritical ? chargedDamage * criticalMultiplier : chargedDamage;

        projectile.ProjectileDamage = finalDamage;
        // 타격음(hitSfx)은 DefaultHitBehavior가 적중(OnHit) 시점에 재생한다.
        projectile.AddHitBehavior(new DefaultHitBehavior(finalDamage, hitSfx, _hitVolume), 100);

        Debug.Log($"[DefaultDamageSO] ({baseDamage} + {levelBonus}) * {chargeMultiplier:F2}" +
                  $"{(isCritical ? $" * {criticalMultiplier:F2}(Critical)" : "")} = {finalDamage:F1}");
    }

    private float GetFinalCriticalMultiplier(float baseCriticalMultiplier)
    {
        float result = baseCriticalMultiplier;

        if (LocalPlayerStatusContext.TryGet(out PlayerStatus playerStatus))
        {
            for (int i = 0; i < playerStatus.CurrentBehaviors.Count; i++)
            {
                ActiveBehavior active = playerStatus.CurrentBehaviors[i];
                if (active.BehaviorSO is CriticalDamageModifierSO critSO)
                {
                    result = critSO.ApplyCriticalMultiplier(result, active.Level);
                }
            }
        }

        return result;
    }
}