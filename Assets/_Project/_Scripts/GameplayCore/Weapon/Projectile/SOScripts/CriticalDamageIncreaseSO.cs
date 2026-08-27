using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Projectile
{
    [CreateAssetMenu(menuName = "Shield Shot/Projectile/Behaviors/Critical Damage Increase", fileName = "CriticalDamageIncreaseSO")]
    public class CriticalDamageIncreaseSO : CriticalDamageModifierSO
    {
        [Header("Critical Damage Increase (크리티컬 데미지 증가)")]
        [Tooltip("1레벨 기준 배율 증가량 (예: 0.2 = +20%p)")]
        [SerializeField, Min(0f)] private float baseBonus = 0.2f;

        [Tooltip("레벨 1당 추가되는 배율 증가량")]
        [SerializeField, Min(0f)] private float bonusPerLevel = 0.1f;

        public override float ApplyCriticalMultiplier(float baseCriticalMultiplier, int level)
        {
            int lvl = Mathf.Max(1, level);
            float bonus = baseBonus + (lvl - 1) * bonusPerLevel;

            return baseCriticalMultiplier + bonus;
        }
    }
}