using Shield_Shot.GameplayCore.Weapon.Projectile;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Modifier
{
    public class NullReflectEffect : IReflectEffect
    {
        public void Execute(ProjectileBase projectile, Vector3 reflectDirection, Collider wallCollider)
        {
            // 기본 반사 외에 추가적인 행동(분열, 유도 등)을 하지 않으므로 비워둡니다.
        }

        public IReflectEffect Clone()
        {
            return new NullReflectEffect();
        }
    }
}

