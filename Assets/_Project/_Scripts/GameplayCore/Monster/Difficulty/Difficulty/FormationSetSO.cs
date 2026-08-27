using System.Collections.Generic;
using Shield_Shot.GameplayCore.Monster.Spawn;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.Difficulty
{
    [System.Serializable]
    public class CountFormationRule
    {
        [Tooltip("이 수 이상일 때 사용")]
        public int minCount;
        public ScriptableObject formation;   // ISpawnFormation 구현체
        [Min(0f)] public float weight = 1f;
    }

    [CreateAssetMenu(fileName = "FormationSet", menuName = "Monster/Formation/Formation Set")]
    public class FormationSetSO : ScriptableObject
    {
        [SerializeField] private List<CountFormationRule> _rules = new();

        public ISpawnFormation GetForCount(int count)
        {
            if (_rules == null || _rules.Count == 0) return null;

            // 현재 카운트에 맞는 가장 높은 minCount 구간 찾기
            int bestMin = -1;
            foreach (var r in _rules)
                if (r.minCount <= count && r.minCount > bestMin) bestMin = r.minCount;
            if (bestMin < 0) return null;

            // 그 구간 내 가중 랜덤
            float total = 0f;
            foreach (var r in _rules) if (r.minCount == bestMin) total += r.weight;

            float rand = Random.Range(0f, total);
            float cum = 0f;
            foreach (var r in _rules)
            {
                if (r.minCount != bestMin) continue;
                cum += r.weight;
                if (rand <= cum) return r.formation as ISpawnFormation;
            }
            return null;
        }
    }
}