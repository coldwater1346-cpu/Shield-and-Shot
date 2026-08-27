using System.Collections;
using Assets._Project._Scripts.GameplayCore.Weapon.Core;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Projectile
{
    [CreateAssetMenu(menuName = "Shield Shot/Projectile/Behaviors/Active Skill/Spread Fire", fileName = "SpreadFireBehaviorSO")]
    public class SpreadFireBehaviorSO : ActiveWeaponSkillSO
    {
        [Header("Spread Fire Settings")]
        [Tooltip("스킬이 뿜어낼 갈래 수 (예: 3갈래)")]
        [SerializeField] private int _projectileCount = 3;

        [Tooltip("스킬의 전체 퍼짐 각도 (도)")]
        [SerializeField, Range(0f, 360f)] private float _spreadAngle = 45f;

        [Header("Damage Penalty")]
        [Tooltip("다중 사격 시 적용할 데미지 배율")]
        [SerializeField, Range(0f, 1f)] private float _damageMultiplier = 0.8f;

        public override void Activate(
            MonoBehaviour coroutineHost,
            IProjectileFireHandler fireHandler,
            Transform firePoint,
            Vector3 aimDirection,
            float chargeRatio,
            int level)
        {
            if (fireHandler == null || firePoint == null) return;

            // 1. 데미지 감소 적용
            StatCalculator statCalc = (fireHandler as MonoBehaviour)?.GetComponent<StatCalculator>()
                ?? (fireHandler as MonoBehaviour)?.GetComponentInChildren<StatCalculator>();

            if (statCalc != null) statCalc.DamageMultiplier = _damageMultiplier;

            // 2. 무기가 샷건(고유 기믹 보유)인지 일반 무기인지 판별
            IWeaponFireExecutor weaponExecutor = fireHandler as IWeaponFireExecutor;

            // 3. 중심축(aimDirection)을 2D 평면 벡터로 변환
            Vector3 baseWorldDir = new Vector3(aimDirection.x, 0f, aimDirection.y).normalized;

            try
            {
                // 스킬 기믹: 설정된 갈래 수(예: 3)만큼 루프를 돌며 방향을 쪼갠다.
                for (int i = 0; i < _projectileCount; i++)
                {
                    float angleOffset = CalculateSkillSpreadAngle(i);
                    Vector3 skillDirection = Quaternion.AngleAxis(angleOffset, Vector3.up) * baseWorldDir;
                    Vector3 finalDirection = new Vector3(skillDirection.x, skillDirection.z, 0f);

                    if (weaponExecutor != null)
                    {
                        // 샷건일 경우: 스킬이 쪼갠 방향(finalDirection)을 중심축으로 던져주면,
                        // 샷건은 그 축을 기준으로 다시 5발의 산탄 기믹을 전개한다. (3갈래 x 5발 = 15발)
                        weaponExecutor.FireFromSkill(firePoint, finalDirection, chargeRatio, false);
                    }
                    else
                    {
                        // 일반 소총일 경우: 스킬이 쪼갠 방향으로 1발씩만 나간다. (3갈래 x 1발 = 3발)
                        fireHandler.Fire(firePoint, finalDirection, chargeRatio, false);
                    }
                }
            }
            finally
            {
                // 4. 데미지 배율 원상복구
                if (statCalc != null) statCalc.DamageMultiplier = 1f;
            }
        }

        private float CalculateSkillSpreadAngle(int index)
        {
            if (_projectileCount <= 1) return 0f;
            float t = (float)index / (_projectileCount - 1);
            return Mathf.Lerp(-_spreadAngle * 0.5f, _spreadAngle * 0.5f, t);
        }
    }
}