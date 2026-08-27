using System.Collections.Generic;
using Shield_Shot.GameplayCore.Render;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Projectile
{
    [CreateAssetMenu(menuName = "Shield Shot/Projectile/Behaviors/Collision FX", fileName = "CollisionFXBehaviorSO")]
    public class CollisionFXBehaviorSO : ProjectileBehaviorSO, ICollisionBehavior
    {
        [Header("VFX Settings")]
        [Tooltip("벽 충돌 시 풀 매니저에서 호출할 VFX 종류 설정")]
        [SerializeField] private VFXType vfxType = VFXType.Reflect;

        [Header("Squash Settings (찌그러짐 연출)")]
        [Tooltip("충돌 순간 진행 방향(길이)이 줄어들 배율 (0.3~0.5 추천. 낮을수록 납작해짐)")]
        [SerializeField] private float squashLengthMultiplier = 0.4f;

        [Tooltip("충돌 순간 옆으로 퍼질 뚱뚱함 배율 (1.3~1.5 추천. 높을수록 양옆으로 퍼짐)")]
        [SerializeField] private float squashWidthMultiplier = 1.4f;

        [Tooltip("찌그러진 상태가 유지될 아주 짧은 시간 (초)")]
        [SerializeField] private float squashDuration = 0.05f;

        public override void InjectBehavior(ProjectileBase projectile, int currentLevel)
        {
            projectile.AddCollisionBehavior(this, Priority);
        }

        public void OnCollide(ProjectileBase projectile, RaycastHit hitInfo)
        {
            if (vfxType != VFXType.None && VFXPoolManager.Instance != null)
            {
                Quaternion hitRotation = Quaternion.LookRotation(hitInfo.normal);
                VFXPoolManager.Instance.SpawnVFX(vfxType, hitInfo.point, hitRotation, 0.5f);
            }

            // 투사체를 순간적으로 쾅 찌그러뜨리는 코루틴 실행
            if (TimeScaleManager.Instance != null && projectile.gameObject.activeInHierarchy)
            {
                TimeScaleManager.Instance.StartCoroutine(Co_SquashPulse(projectile));
            }
        }

        private IEnumerator<WaitForSecondsRealtime> Co_SquashPulse(ProjectileBase projectile)
        {
            if (projectile == null || !projectile.gameObject.activeInHierarchy) yield break;

            // 원래 고유 스케일 백업
            Vector3 originalScale = projectile.transform.localScale;

            // 찌러뜨리기 연산
            Vector3 squashedScale = new Vector3(
                originalScale.x * squashWidthMultiplier,
                originalScale.y * squashWidthMultiplier,
                originalScale.z * squashLengthMultiplier
            );

            // 찌그러진 크기 대입
            projectile.transform.localScale = squashedScale;

            // 미세 시간 대기 (현실 시간 기준)
            yield return new WaitForSecondsRealtime(squashDuration);

            // 대기 후 화살이 아직 파괴되지 않고 살아있다면 원상태로 복구
            if (projectile != null && projectile.gameObject.activeInHierarchy)
            {
                projectile.transform.localScale = originalScale;
            }
        }
    }
}