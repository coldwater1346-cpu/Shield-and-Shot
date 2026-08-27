using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Modifier.Reflect
{
    // 반사 방향 계산 방식을 정의하는 인터페이스
    public interface IReflectBehavior
    {
        // 반사 방향을 계산해 반환한다
        Vector3 Calculate(Vector3 incomingDirection, Vector3 surfaceNormal);
    }
}