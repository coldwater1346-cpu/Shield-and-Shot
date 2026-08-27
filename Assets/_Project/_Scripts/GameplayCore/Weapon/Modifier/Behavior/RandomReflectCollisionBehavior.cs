using Shield_Shot.GameplayCore.Weapon.Projectile;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Projectile
{
    public class RandomReflectCollisionBehavior : ICollisionBehavior, ICopyableCollisionBehavior
    {
        private readonly int _level;
        private const float WallSkin = 0.03f;

        public RandomReflectCollisionBehavior(int level)
        {
            _level = Mathf.Max(1, level);
        }

        public ICollisionBehavior CreateCopy()
        {
            return new RandomReflectCollisionBehavior(_level);
        }

        public void OnCollide(ProjectileBase projectile, RaycastHit hitInfo)
        {
            if (projectile == null) return;

            Vector3 surfaceNormal = hitInfo.normal;
            surfaceNormal.y = 0f;
            surfaceNormal = surfaceNormal.sqrMagnitude > 0.0001f
                ? surfaceNormal.normalized
                : Vector3.forward;

            // StandardReflect may already have changed Velocity. RandomReflect must override
            // from the wall normal, so it always leaves within the outside 180-degree arc.
            float maxAngle = Mathf.Min(90f, 45f + (_level - 1) * 15f);
            float randomAngle = Random.Range(-maxAngle, maxAngle);
            Vector3 finalReflectDir = Quaternion.AngleAxis(randomAngle, Vector3.up) * surfaceNormal;
            finalReflectDir.y = 0f;
            finalReflectDir = finalReflectDir.sqrMagnitude > 0.0001f
                ? finalReflectDir.normalized
                : surfaceNormal;

            float speed = projectile.Velocity.magnitude > 0.0001f
                ? projectile.Velocity.magnitude
                : projectile.BaseSpeed;

            projectile.transform.position = hitInfo.point + surfaceNormal * (projectile.ProjectileRadius + WallSkin);
            projectile.Velocity = finalReflectDir * speed;
        }
    }
}
