using Shield_Shot.GameplayCore.Weapon.Projectile;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Projectile
{
    public class SplitCollisionBehavior : ICollisionBehavior
    {
        private readonly int _level;
        private readonly int _childCount;
        public const float SplitAngle = 18f;
        public const float WallSkin = 0.03f;

        public SplitCollisionBehavior(int level)
        {
            _level = level;
            _childCount = Mathf.Max(1, level + 1); // 레벨1=2발, 레벨2=3발 ...
        }

        public void OnCollide(ProjectileBase projectile, RaycastHit hitInfo)
        {
            if (projectile.TryRequestExternalSplit(hitInfo, _childCount))
            {
                projectile.ReleaseOrDestroy();
                return;
            }

            Vector3 baseDirection = projectile.Velocity.sqrMagnitude > 0.0001f
                ? projectile.Velocity.normalized
                : projectile.transform.forward;
            float speed = projectile.Velocity.magnitude > 0.0001f
                ? projectile.Velocity.magnitude
                : projectile.BaseSpeed;

            for (int i = 0; i < _childCount; i++)
            {
                Vector3 childIncomingDirection = GetChildIncomingDirection(baseDirection, i, _childCount);
                Vector3 spawnPosition = GetChildSpawnPosition(hitInfo, projectile.ProjectileRadius);

                ProjectileBase childProjectile = SpawnChildProjectile(projectile, spawnPosition);
                if (childProjectile == null)
                {
                    continue;
                }

                childProjectile.BaseSpeed = projectile.BaseSpeed;
                childProjectile.ChargeRatio = projectile.ChargeRatio;
                childProjectile.Velocity = childIncomingDirection * speed;
                childProjectile.ProjectileDamage = projectile.ProjectileDamage;
                childProjectile.IsCritical = projectile.IsCritical;
                childProjectile.SourceWeaponType = projectile.SourceWeaponType;

                // 핵심 1: 분열은 1회성으로 소모하고, 반사/난반사 같은 나머지 특성만 자식에게 복사
                projectile.CopyBehaviorsTo(childProjectile, typeof(SplitCollisionBehavior));

                // 핵심 2: 자식에게 부모의 충돌 정보를 패스스루(Pass-through)하여 강제 충돌 이벤트를 실행!
                childProjectile.ExecuteCollision(hitInfo);
            }

            // 역할을 마친 부모 투사체는 월드에서 안전하게 소멸
            projectile.ReleaseOrDestroy();
        }

        // 부모와 같은 풀(WeaponType 기준)에서 재사용 가능한 투사체를 빌려온다.
        // 풀을 못 구하면(테스트 환경, 매핑 안 된 WeaponType 등) 기존처럼 직접 Instantiate로 폴백한다.
        private static ProjectileBase SpawnChildProjectile(ProjectileBase parent, Vector3 spawnPosition)
        {
            if (ProjectileManager.Instance != null)
            {
                ProjectileBase pooled = ProjectileManager.Instance.GetProjectile(
                    parent.SourceWeaponType, spawnPosition, parent.transform.rotation);

                if (pooled != null)
                {
                    return pooled;
                }
            }

            GameObject childObj = Object.Instantiate(parent.gameObject, spawnPosition, parent.transform.rotation);
            ProjectileBase fallback = childObj.GetComponent<ProjectileBase>();
            if (fallback == null)
            {
                Object.Destroy(childObj);
                return null;
            }

            fallback.ResetProjectileState();
            fallback.SetUpdateSimulationEnabled(true);
            fallback.enabled = true;

            // 이번엔 풀에서 못 꺼냈어도, 반환 시엔 해당 무기 타입 풀로 편입시켜서
            // 다음번부터는 정상적으로 풀링(재사용)되게 한다.
            if (ProjectileManager.Instance != null &&
                ProjectileManager.Instance.TryGetPool(parent.SourceWeaponType, out ProjectileObjectPool pool))
            {
                fallback.SetReturnCallback(pool.Return);
            }

            return fallback;
        }

        public static Vector3 GetChildIncomingDirection(Vector3 baseDirection, int childIndex, int childCount)
        {
            baseDirection.y = 0f;
            baseDirection = baseDirection.sqrMagnitude > 0.0001f
                ? baseDirection.normalized
                : Vector3.forward;

            // 자식 수가 몇 개든 중앙 기준 대칭으로 부채꼴 배분 (홀수: 정중앙 포함, 짝수: 정중앙 없이 좌우 대칭)
            float center = (childCount - 1) / 2f;
            float angle = (childIndex - center) * SplitAngle;
            Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * baseDirection;
            direction.y = 0f;
            return direction.sqrMagnitude > 0.0001f ? direction.normalized : baseDirection;
        }

        public static Vector3 GetChildSpawnPosition(RaycastHit hitInfo, float projectileRadius)
        {
            Vector3 surfaceNormal = hitInfo.normal;
            surfaceNormal.y = 0f;
            surfaceNormal = surfaceNormal.sqrMagnitude > 0.0001f
                ? surfaceNormal.normalized
                : Vector3.forward;

            return hitInfo.point + surfaceNormal * (Mathf.Max(0f, projectileRadius) + WallSkin);
        }
    }
}