using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.Movement
{
    [CreateAssetMenu(fileName = "Movement_Zigzag", menuName = "Monster/Movement/Zigzag")]
    public class ZigzagMovement : ScriptableObject, IMovementBehavior
    {
        [Tooltip("좌우 흔들림 주기(Hz). 클수록 빠르게 흔들림")]
        [SerializeField] private float _frequency = 2f;
        [Tooltip("좌우 흔들림 진폭. 클수록 좌우로 멀리 흔들림")]
        [SerializeField] private float _amplitude = 3f;

        public Vector3 CalculateVelocity(Vector3 currentVelocity, float baseSpeed, Transform transform, float time)
        {
            // 인스턴스별 위상 오프셋 (같은 SO여도 몬스터마다 다른 타이밍)
            float instancePhase = (transform.GetInstanceID() % 1000) / 1000f * Mathf.PI * 2f;

            // Perlin noise → 불규칙하지만 부드러운 움직임 (사인파보다 생동감)
            float noiseX = time * _frequency + instancePhase;
            float noise = (Mathf.PerlinNoise(noiseX, 0f) * 2f - 1f); // -1 ~ 1

            // Vector3.right → transform.right (몬스터 방향 기준)
            Vector3 lateral = transform.right * noise * _amplitude;
            return currentVelocity + lateral;
        }
    }
}   