using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.Movement
{
    [CreateAssetMenu(fileName = "WanderMovement", menuName = "Monster/Movement/Wander")]
    public class WanderMovement : ScriptableObject, IMovementBehavior
    {
        [SerializeField] private float _strength = 0.15f;   // 좌우 흔들림 강도 (baseSpeed 배율)
        [SerializeField] private float _frequency = 0.8f;   // 흔들림 속도 (낮을수록 완만)

        public Vector3 CalculateVelocity(Vector3 currentVelocity, float baseSpeed, Transform transform, float time)
        {
            // 인스턴스별 위상 오프셋
            float instancePhase = (transform.GetInstanceID() % 1000) / 1000f * 100f;

            // Perlin noise로 부드러운 랜덤 흔들림
            float noise = (Mathf.PerlinNoise(time * _frequency + instancePhase, 0f) * 2f - 1f);

            return currentVelocity + transform.right * noise * _strength * baseSpeed;
        }
    }
}