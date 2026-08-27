using Shield_Shot.GameplayCore.Render;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Projectile
{
    [CreateAssetMenu(menuName = "Shield Shot/Projectile/Behaviors/Hit FX", fileName = "HitFXBehaviorSO")]
    public class HitFXBehaviorSO : ProjectileBehaviorSO, IHitBehavior
    {
        [Header("Hit Stop Settings")]
        [Tooltip("적 충돌 시 히트 스톱 지속 시간 (초)")]
        [SerializeField] private float hitStopDuration = 0.12f;

        [Tooltip("히트 스톱 순간의 시간 속도 (0.02 ~ 0.1 추천)")]
        [SerializeField] private float hitStopScale = 0.05f;

        [Header("VFX Settings")]
        [Tooltip("적중 시 풀 매니저에서 호출할 VFX 종류 설정")]
        [SerializeField] private VFXType vfxType = VFXType.Hit;

        public override void InjectBehavior(ProjectileBase projectile, int currentLevel)
        {
            projectile.AddHitBehavior(this, Priority);
        }

        public void OnHit(ProjectileBase projectile, Collider targetInfo)
        {
            bool isCriticalHit = projectile.IsCritical;

            if (TimeScaleManager.Instance != null)
            {
                TimeScaleManager.Instance.RequestHitStop(isCriticalHit, hitStopDuration, hitStopScale);
            }

            if (vfxType != VFXType.None && VFXPoolManager.Instance != null)
            {
                Vector3 spawnPosition = projectile.transform.position;

                if (targetInfo != null)
                {
                    spawnPosition = targetInfo.ClosestPoint(projectile.transform.position);
                }

                if (spawnPosition == Vector3.zero && projectile.transform.position != Vector3.zero)
                {
                    spawnPosition = projectile.transform.position;
                }

                // 풀 매니저를 통해 가비지 없이 이펙트 재생 (자동 반환 시간 1.5초 전달)
                VFXPoolManager.Instance.SpawnVFX(vfxType, spawnPosition, projectile.transform.rotation, 1.5f);
            }
        }
    }
}