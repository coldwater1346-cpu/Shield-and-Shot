using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Modifier.Reflect
{
    // Vector3.Reflect를 그대로 사용하는 가장 기본적인 반사
    public class SpecularReflectBehavior : IReflectBehavior
    {
        public Vector3 Calculate(Vector3 incomingDirection, Vector3 surfaceNormal)
        {
            Vector3 reflected = Vector3.Reflect(incomingDirection, surfaceNormal);
            reflected.y = 0f; // 탑다운 XZ 평면 유지
            return reflected.sqrMagnitude > 0.0001f ? reflected.normalized : Vector3.forward;
        }
    }
}