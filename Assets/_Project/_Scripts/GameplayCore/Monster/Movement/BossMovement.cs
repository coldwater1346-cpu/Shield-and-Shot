using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.Movement
{
    [CreateAssetMenu(fileName = "BossMovement", menuName = "Monster/Movement/BossMovement")]
    public class BossMovement : ScriptableObject, IMovementBehavior
    {
        [Header("이동 범위 (스폰 지점 기준 ±)")]
        [SerializeField] private float _rangeX = 4f;   // 좌우 폭
        [SerializeField] private float _rangeZ = 2f;   // 앞뒤 폭

        [Header("배회 속도 (X·Z를 다르게 줘야 자유롭게 보임)")]
        [SerializeField] private float _speedX = 1.2f;
        [SerializeField] private float _speedZ = 0.8f;

        public Vector3 CalculateVelocity(Vector3 currentVelocity, float baseSpeed,
                                         Transform transform, float time)
        {
            // 위치 진폭이 range가 되도록 속도 = range * speed * cos(...)
            float vx = _rangeX * _speedX * Mathf.Cos(time * _speedX);
            float vz = _rangeZ * _speedZ * Mathf.Cos(time * _speedZ + 1.3f); // 위상차로 X·Z 어긋나게
            return currentVelocity + new Vector3(vx, 0f, vz);
        }
    }
}