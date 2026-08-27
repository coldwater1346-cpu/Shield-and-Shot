using System.Collections.Generic;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.Difficulty
{
    /// <summary>
    /// 몬스터 "종별" 성장 성향. 같은 난이도라도 몬스터마다 다르게 크도록 하는 가중치.
    ///
    /// 난이도 프로파일이 "공통 수식"이라면, 이건 몬스터별 "성장 배분표"다.
    ///   - 탱커: HP 가중치 1.6, 이동속도 0.4  → 난이도가 올라도 느리고 아주 단단해짐
    ///   - 스카우트: HP 0.6, 이동속도 1.5     → 물렁하지만 점점 빨라짐
    ///
    /// 가중치 의미 (리졸버의 보간식):
    ///   final = base + (scaled - base) × weight
    ///     weight 0  → 성장 안 함(항상 base)
    ///     weight 1  → 프로파일 수식 그대로
    ///     weight>1  → 프로파일보다 더 가파르게 성장
    ///
    /// 이 SO 없이(null) 스폰하면 모든 가중치 1 로 간주 = 프로파일 수식 그대로.
    /// </summary>
    [CreateAssetMenu(fileName = "MonsterStatProfile", menuName = "Monster/Difficulty/Monster Stat Profile")]
    public class MonsterStatProfileSO : ScriptableObject
    {
        [System.Serializable]
        public struct GrowthWeight
        {
            public StatType stat;
            [Tooltip("이 스탯의 성장 가중치. 1=프로파일 그대로, 0=성장 안 함")]
            public float weight;
        }

        [SerializeField] private List<GrowthWeight> _weights = new();

        [Tooltip("목록에 없는 스탯에 적용할 기본 가중치")]
        [SerializeField] private float _defaultWeight = 1f;

        private float[] _lookup;

        private void BuildLookup()
        {
            _lookup = new float[(int)StatType.Count];
            for (int i = 0; i < _lookup.Length; i++) _lookup[i] = _defaultWeight;
            if (_weights == null) return;
            foreach (var w in _weights)
            {
                int i = (int)w.stat;
                if (i >= 0 && i < _lookup.Length) _lookup[i] = w.weight;
            }
        }

        private void OnEnable() => BuildLookup();

        public float GetGrowthWeight(StatType stat)
        {
            if (_lookup == null) BuildLookup();
            int i = (int)stat;
            return (i >= 0 && i < _lookup.Length) ? _lookup[i] : _defaultWeight;
        }
    }
}
