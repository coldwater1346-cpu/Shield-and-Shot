using Shield_Shot.GameplayCore.Weapon.Projectile;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Shield
{
    public interface IShieldEffect
    {
        void OnBlock(ProjectileBase projectile, Vector3 hitPosition, Vector3 hitNormal);
    }
}