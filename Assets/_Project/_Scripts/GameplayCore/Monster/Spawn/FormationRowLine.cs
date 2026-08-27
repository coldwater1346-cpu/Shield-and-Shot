// Data/Formation_RowLine.cs  (가로 일렬)
using System.Collections.Generic;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.Spawn
{
    [CreateAssetMenu(menuName = "Monster/Formation/RowLine")]
    public class Formation_RowLine : ScriptableObject, ISpawnFormation
    {
        [SerializeField] private float _spacing = 2f;

        public List<Vector3> CalculatePositions(Vector3 centerPosition, int count)
        {
            var result = new List<Vector3>(count);
            float totalWidth = _spacing * (count - 1);
            for (int i = 0; i < count; i++)
            {
                float x = -totalWidth * 0.5f + _spacing * i;
                result.Add(centerPosition + new Vector3(x, 0f, 0f));
            }
            return result;
        }
    }
}