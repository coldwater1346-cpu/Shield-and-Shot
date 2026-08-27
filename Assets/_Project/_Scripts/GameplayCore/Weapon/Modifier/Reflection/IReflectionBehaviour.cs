using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Modifier
{
    public interface IReflectionBehavior
    {
        // 입력 방향과 법선 벡터를 받아 최종 반사 방향을 반환
        Vector3 CalculateDirection(Vector3 incomingDirection, Vector3 surfaceNormal);
        IReflectionBehavior Clone();
    }
}