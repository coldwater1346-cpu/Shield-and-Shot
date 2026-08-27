using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Projectile
{
    public interface ICollisionBehavior
    {
        void OnCollide(ProjectileBase projectile, RaycastHit hitInfo);
    }

    public interface ICopyableCollisionBehavior
    {
        ICollisionBehavior CreateCopy();
    }
}
