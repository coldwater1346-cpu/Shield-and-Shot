using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Projectile
{
    public interface IMovementBehavior
    {
        void UpdateMovement(Transform projectileTransform, ref Vector3 velocity, float deltaTime);
    }

}