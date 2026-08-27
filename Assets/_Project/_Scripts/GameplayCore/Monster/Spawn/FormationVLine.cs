// Data/Formation_VLine.cs  (V자 대형)
using System.Collections.Generic;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.Spawn
{
    [CreateAssetMenu(menuName = "Monster/Formation/VLine")]
    public class FormationVLine : ScriptableObject, ISpawnFormation
    {
        [SerializeField] private float _spacingX = 2f;
        [SerializeField] private float _spacingZ = 1.5f;  // V의 깊이

        public List<Vector3> CalculatePositions(Vector3 centerPosition, int count)
        {
            var result = new List<Vector3>(count);
            for (int i = 0; i < count; i++)
            {
                int half = count / 2;
                float x = (i - half) * _spacingX;
                float z = -Mathf.Abs(i - half) * _spacingZ;  // 양 끝이 앞으로 나옴
                result.Add(centerPosition + new Vector3(x, 0f, z));
            }
            return result;
        }
    }
}