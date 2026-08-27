using System.Collections.Generic;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.Spawn
{
    public interface ISpawnFormation
    {
        /// <summary>centerPosition 기준으로 count개의 스폰 좌표를 반환.</summary>
        List<Vector3> CalculatePositions(Vector3 centerPosition, int count);
    }
}