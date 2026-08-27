using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Projectile
{
    [CreateAssetMenu(menuName = "Shield Shot/Projectile/Behaviors/Attack Speed Increase", fileName = "AttackSpeedIncreaseSO")]
    public class AttackSpeedIncreaseSO : AttackSpeedModifierSO
    {
        [Header("Attack Speed Increase (공격속도 증가)")]
        [Tooltip("1레벨 기준 쿨타임 감소율 (%)")]
        [SerializeField, Range(0f, 90f)] private float basePercent = 10f;

        [Tooltip("레벨 1당 추가로 줄어드는 쿨타임 감소율 (%)")]
        [SerializeField, Range(0f, 50f)] private float percentPerLevel = 5f;

        [Tooltip("감소율 상한 (%). 쿨타임이 0에 가까워져 연사가 무한대로 빨라지는 것을 막는 안전장치.")]
        [SerializeField, Range(0f, 95f)] private float maxPercent = 70f;

        [Tooltip("결과 쿨타임의 최소값 (초). 이 값보다 더 짧아지지 않는다.")]
        [SerializeField, Min(0.01f)] private float minCooldown = 0.05f;

        public override float ApplyCooldown(float baseCooldown, int level)
        {
            int lvl = Mathf.Max(1, level);
            float reducePercent = Mathf.Min(maxPercent, basePercent + (lvl - 1) * percentPerLevel);
            float multiplier = 1f - reducePercent / 100f;

            return Mathf.Max(minCooldown, baseCooldown * multiplier);
        }
    }
}