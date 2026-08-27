using Shield_Shot.GameplayCore.Weapon.Projectile;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Shield
{
    public interface IShieldColliderDetector
    {
        event System.Action<ProjectileBase, Vector3> OnProjectileDetected;
    }
}