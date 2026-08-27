using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.Movement
{
    [CreateAssetMenu(fileName = "Movement_Straight", menuName = "Monster/Movement/Straight")]
    public class MovementData_Straight : ScriptableObject, IMovementBehavior
    {
        public Vector3 CalculateVelocity(Vector3 currentVelocity, float baseSpeed, Transform transform, float time)
        {
            return transform.forward * baseSpeed;  // direction 필드 제거, 몬스터 정면 방향으로
        }
    }
}