// Data/Formation_Single.cs
using System.Collections.Generic;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.Spawn
{
    [CreateAssetMenu(menuName = "Monster/Formation/Single")]
    public class FormationSingle : ScriptableObject, ISpawnFormation
    {
        public List<Vector3> CalculatePositions(Vector3 centerPosition, int count)
        {
            var result = new List<Vector3>(count);
            for (int i = 0; i < count; i++)
                result.Add(centerPosition);
            return result;
        }
    }
}