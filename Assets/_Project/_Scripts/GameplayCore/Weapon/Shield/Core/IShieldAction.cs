using Shield_Shot.GameplayCore.Weapon.Projectile;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Shield
{
    public interface IShieldAction
    {
        // 방패의 기본 방어 규정
        void OnProjectileHit(ProjectileBase projectile, Vector3 hitNormal);
    }
}