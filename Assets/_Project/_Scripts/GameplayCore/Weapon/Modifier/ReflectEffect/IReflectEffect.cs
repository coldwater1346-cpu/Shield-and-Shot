using Shield_Shot.GameplayCore.Weapon.Projectile;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Modifier
{
    public interface IReflectEffect
    {
        // 반사가 일어난 직후, 계산된 방향과 충돌 정보를 바탕으로 부가 효과 수행
        void Execute(ProjectileBase projectile, Vector3 reflectDirection, Collider wallCollider);
        IReflectEffect Clone();
    }
}

