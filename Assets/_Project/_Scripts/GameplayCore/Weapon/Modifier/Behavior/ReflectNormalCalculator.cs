using UnityEngine;
using Shield_Shot.GameplayCore.Weapon.Projectile;

namespace Shield_Shot.GameplayCore.Weapon.Modifier.Reflect
{
    // 충돌 표면 법선 계산 전담 클래스
    public static class ReflectNormalCalculator
    {
        private const float CastRadius = 0.1f;
        private const float CastDistance = 1.0f;

        // 투사체와 충돌한 벽 콜라이더로부터 표면 법선을 획득
        public static Vector3 GetSurfaceNormal(ProjectileBase projectile, Collider wallCollider)
        {
            // 1순위: SphereCast로 정확한 충돌 법선 획득
            if (Physics.SphereCast(
                    origin: projectile.transform.position,
                    radius: CastRadius,
                    direction: projectile.Direction,
                    hitInfo: out RaycastHit hit,
                    maxDistance: CastDistance,
                    layerMask: 1 << wallCollider.gameObject.layer))
            {
                return hit.normal;
            }

            // 2순위: ClosestPoint 기반 역방향 (SphereCast 실패 시)
            Vector3 closest = wallCollider.ClosestPoint(projectile.transform.position);
            Vector3 toWall = closest - projectile.transform.position;
            return toWall.sqrMagnitude > 0.0001f ? -toWall.normalized : Vector3.up;
        }
    }
}