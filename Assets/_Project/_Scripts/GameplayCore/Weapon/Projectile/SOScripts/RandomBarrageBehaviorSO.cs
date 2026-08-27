using System.Collections;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Projectile
{
    [CreateAssetMenu(menuName = "Shield Shot/Projectile/Behaviors/Active Skill/Random Barrage", fileName = "RandomBarrageBehaviorSO")]
    public class RandomBarrageBehaviorSO : ActiveWeaponSkillSO
    {
        [Header("Random Barrage (난사 사격)")]
        [Tooltip("1레벨 기준 총 발사 수")]
        [SerializeField, Min(1)] private int baseShotCount = 10;

        [Tooltip("레벨 1당 추가되는 발사 수")]
        [SerializeField, Min(0)] private int extraShotsPerLevel = 2;

        [Tooltip("탄환이 퍼질 수 있는 전체 무작위 각도 범위")]
        [SerializeField, Range(0f, 180f)] private float spreadAngle = 90f;

        [Tooltip("연사 속도 (초)")]
        [SerializeField, Min(0f)] private float shotInterval = 0.08f;

        [Header("Damage Penalty")]
        [Tooltip("데미지 배율")]
        [SerializeField, Range(0f, 1f)] private float damageMultiplier = 0.6f;

        public override void Activate(
            MonoBehaviour coroutineHost,
            IProjectileFireHandler fireHandler,
            Transform firePoint,
            Vector3 aimDirection,
            float chargeRatio,
            int level)
        {
            if (coroutineHost == null || fireHandler == null || firePoint == null)
            {
                Debug.LogWarning("[RandomBarrageBehaviorSO] 발동 실패 - 필수 참조 누락.");
                return;
            }

            int lvl = Mathf.Max(1, level);
            int totalShots = baseShotCount + (lvl - 1) * extraShotsPerLevel;

            coroutineHost.StartCoroutine(Co_FireRandomBarrage(fireHandler, firePoint, aimDirection, chargeRatio, totalShots));
        }

        private IEnumerator Co_FireRandomBarrage(
            IProjectileFireHandler fireHandler,
            Transform firePoint,
            Vector3 aimDirection,
            float chargeRatio,
            int shotCount)
        {
            // 1. fireHandler(무기 오브젝트) 컴포넌트에서 StatCalculator를 탐색한다.
            StatCalculator statCalc = (fireHandler as MonoBehaviour)?.GetComponent<StatCalculator>()
                ?? (fireHandler as MonoBehaviour)?.GetComponentInChildren<StatCalculator>();

            // 2. 난사 시작 전 데미지 배율을 0.4배(60% 감소)로 세팅한다.
            if (statCalc != null)
            {
                statCalc.DamageMultiplier = damageMultiplier;
            }

            // 3. 안전한 복구를 위해 try-finally 구조 안에서 사격을 실행한다.
            try
            {
                Vector3 baseWorldDir = new Vector3(aimDirection.x, 0f, aimDirection.y).normalized;

                for (int i = 0; i < shotCount; i++)
                {
                    float randomAngle = Random.Range(-spreadAngle * 0.5f, spreadAngle * 0.5f);
                    Vector3 rotatedWorldDir = Quaternion.AngleAxis(randomAngle, Vector3.up) * baseWorldDir;
                    Vector3 randomDirectionForFire = new Vector3(rotatedWorldDir.x, rotatedWorldDir.z, 0f);

                    // 이제 여기서 넘기는 chargeRatio는 원래 무기 로직(크리티컬 등)대로 작동하고, 
                    // 최종 데미지만 내부적으로 60% 깎여서 발사된다.
                    fireHandler.Fire(firePoint, randomDirectionForFire, chargeRatio, false);

                    yield return new WaitForSeconds(shotInterval);
                }
            }
            finally
            {
                // 4. 사격 연사가 끝나거나, 중간에 스킬이 취소되더라도 무기의 데미지 배율을 원래대로(1.0) 반드시 복구한다.
                if (statCalc != null)
                {
                    statCalc.DamageMultiplier = 1f;
                }
                Debug.Log("[RandomBarrageBehaviorSO] 난사 사격 종료 및 무기 스탯 원상복구 완료.");
            }
        }
    }
}