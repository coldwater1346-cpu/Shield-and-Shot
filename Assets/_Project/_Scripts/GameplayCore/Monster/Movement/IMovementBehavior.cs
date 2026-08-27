using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.Movement
{
    public interface IMovementBehavior
    {
        /// <summary>
        /// 몬스터의 이동 속도를 계산하여 반환한다.
        /// </summary>
        /// <param name="currentVelocity">현재 속도</param>
        /// <param name="baseSpeed">MonsterDataSO의 기본 속도</param>
        /// <param name="transform">몬스터의 Transform</param>
        /// <param name="time">경과 시간 또는 프레임 타임</param>
        Vector3 CalculateVelocity(Vector3 currentVelocity, float baseSpeed, Transform transform, float time);
    }
}
