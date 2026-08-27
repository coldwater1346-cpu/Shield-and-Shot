using System.Collections;
using Assets._Project._Scripts.GameplayCore.Weapon.Core;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Projectile
{
    [CreateAssetMenu(menuName = "Shield Shot/Projectile/Behaviors/Active Skill/Burst Fire", fileName = "BurstFireBehaviorSO")]
    public class BurstFireBehaviorSO : ActiveWeaponSkillSO
    {
        [Header("Burst Fire Settings")]
        [Tooltip("점사 발사 횟수 (예: 3번)")]
        [SerializeField] private int _burstCount = 3;

        [Tooltip("발사 간격 (초)")]
        [SerializeField] private float _burstInterval = 0.08f;

        [Header("Damage Penalty")]
        [Tooltip("점사 시 적용할 데미지 배율")]
        [SerializeField, Range(0f, 1f)] private float _damageMultiplier = 1f;

        public override void Activate(
            MonoBehaviour coroutineHost,
            IProjectileFireHandler fireHandler,
            Transform firePoint,
            Vector3 aimDirection,
            float chargeRatio,
            int level)
        {
            if (coroutineHost == null || fireHandler == null || firePoint == null) return;
            coroutineHost.StartCoroutine(Co_ExecuteBurstFire(fireHandler, firePoint, aimDirection, chargeRatio));
        }

        private IEnumerator Co_ExecuteBurstFire(
            IProjectileFireHandler fireHandler,
            Transform firePoint,
            Vector3 aimDirection,
            float chargeRatio)
        {
            StatCalculator statCalc = (fireHandler as MonoBehaviour)?.GetComponent<StatCalculator>()
                ?? (fireHandler as MonoBehaviour)?.GetComponentInChildren<StatCalculator>();

            if (statCalc != null) statCalc.DamageMultiplier = _damageMultiplier;

            try
            {
                // 순수하게 전달받은 파이어 핸들러를 향해 일정 간격으로 사격 명령만 내린다.
                for (int i = 0; i < _burstCount; i++)
                {
                    fireHandler.Fire(firePoint, aimDirection, chargeRatio, false);
                    yield return new WaitForSeconds(_burstInterval);
                }
            }
            finally
            {
                if (statCalc != null) statCalc.DamageMultiplier = 1f;
            }
        }
    }
}