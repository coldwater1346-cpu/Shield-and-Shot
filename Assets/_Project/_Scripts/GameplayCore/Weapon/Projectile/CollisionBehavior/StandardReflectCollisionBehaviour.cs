using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Projectile
{
    public class StandardReflectCollisionBehavior : ICollisionBehavior, ICopyableCollisionBehavior
    {
        private const float WallSkin = 0.03f;

        private int _remainingReflectCount;

        public int ReflectCount
        {
            get => _remainingReflectCount;
            set => _remainingReflectCount = value;
        }

        public StandardReflectCollisionBehavior(int level)
        {
            _remainingReflectCount = Mathf.Max(0, level);
        }

        public ICollisionBehavior CreateCopy()
        {
            return new StandardReflectCollisionBehavior(_remainingReflectCount);
        }

        public void OnCollide(ProjectileBase projectile, RaycastHit hitInfo)
        {
            if (projectile == null) return;

            if (_remainingReflectCount <= 0)
            {
                projectile.ReleaseOrDestroy();
                return;
            }
            Debug.Log("[StandardReflect] Collided");
            Vector3 incomingDirection = projectile.Velocity.sqrMagnitude > 0.0001f
                ? projectile.Velocity.normalized
                : projectile.transform.forward;

            Vector3 surfaceNormal = hitInfo.normal;
            surfaceNormal.y = 0f;
            surfaceNormal = surfaceNormal.sqrMagnitude > 0.0001f
                ? surfaceNormal.normalized
                : Vector3.forward;

            Vector3 reflectedDir = Vector3.Reflect(
                incomingDirection,
                surfaceNormal
            );

            reflectedDir.y = 0f;
            reflectedDir = reflectedDir.sqrMagnitude > 0.0001f
                ? reflectedDir.normalized
                : Vector3.forward;

            float speed = projectile.Velocity.magnitude > 0.0001f
                ? projectile.Velocity.magnitude
                : projectile.BaseSpeed;

            projectile.transform.position = hitInfo.point + surfaceNormal * (projectile.ProjectileRadius + WallSkin);
            projectile.Velocity = reflectedDir * speed;
            _remainingReflectCount--;
        }
    }
}
