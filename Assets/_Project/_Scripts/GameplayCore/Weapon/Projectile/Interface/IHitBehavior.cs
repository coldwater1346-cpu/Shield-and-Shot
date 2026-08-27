using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Projectile
{
    public interface IHitBehavior
    {
        void OnHit(ProjectileBase projectile, Collider targetInfo);
    }

    public interface ICopyableHitBehavior
    {
        IHitBehavior CreateCopy();
    }

    public interface IProjectileHitSurvivalBehavior
    {
    }
}

