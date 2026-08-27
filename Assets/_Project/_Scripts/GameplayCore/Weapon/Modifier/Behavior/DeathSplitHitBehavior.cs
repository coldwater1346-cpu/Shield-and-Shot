using Shield_Shot.GameplayCore.Monster.Core;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Projectile
{
    public class DeathSplitHitBehavior : IHitBehavior
    {
        private readonly int _childCount;
        private readonly float _damageMultiplier;

        public DeathSplitHitBehavior(int childCount, float damageMultiplier)
        {
            _childCount = Mathf.Max(1, childCount);
            _damageMultiplier = Mathf.Clamp01(damageMultiplier);
        }

        public void OnHit(ProjectileBase projectile, Collider targetInfo)
        {
            if (projectile == null || targetInfo == null) return;

            // 데미지가 이미 적용된 뒤(Priority=150)라 IsDead로 처치 여부를 바로 판단 가능
            HealthComponent health = targetInfo.GetComponentInParent<HealthComponent>();
            if (health == null || !health.IsDead) return;

            Vector3 spawnPosition = projectile.transform.position;
            float childDamage = projectile.ProjectileDamage * _damageMultiplier;
            float speed = projectile.Velocity.magnitude > 0.0001f
                ? projectile.Velocity.magnitude
                : projectile.BaseSpeed;

            for (int i = 0; i < _childCount; i++)
            {
                Vector3 randomDirection = RandomXZDirection();

                ProjectileBase childProjectile = SpawnChildProjectile(projectile, spawnPosition);
                if (childProjectile == null) continue;

                childProjectile.BaseSpeed = projectile.BaseSpeed;
                childProjectile.ChargeRatio = projectile.ChargeRatio;
                childProjectile.Velocity = randomDirection * speed;
                childProjectile.IsCritical = false;
                childProjectile.SourceWeaponType = projectile.SourceWeaponType;

                projectile.CopyBehaviorsTo(childProjectile);

                // CopyBehaviorsTo가 원본 DefaultHitBehavior를 그대로 복사해오므로,
                // 여기서 실제 데미지 값 자체를 다시 깎아줘야 함 (ProjectileDamage 프로퍼티만 바꿔선 안 먹음)
                DefaultHitBehavior damageBehavior = childProjectile.FindHitBehavior<DefaultHitBehavior>();
                if (damageBehavior != null)
                {
                    damageBehavior.Damage *= _damageMultiplier;
                    childProjectile.ProjectileDamage = damageBehavior.Damage;
                }
                else
                {
                    childProjectile.ProjectileDamage = childDamage;
                }
            }
        }

        // 부모와 같은 풀(WeaponType 기준)에서 재사용 가능한 투사체를 빌려온다.
        private static ProjectileBase SpawnChildProjectile(ProjectileBase parent, Vector3 spawnPosition)
        {
            if (ProjectileManager.Instance != null)
            {
                ProjectileBase pooled = ProjectileManager.Instance.GetProjectile(
                    parent.SourceWeaponType, spawnPosition, parent.transform.rotation);

                if (pooled != null) return pooled;
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

        private static Vector3 RandomXZDirection()
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            return new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
        }
    }
}