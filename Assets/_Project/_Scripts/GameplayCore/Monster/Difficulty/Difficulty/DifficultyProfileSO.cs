using System.Collections.Generic;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.Difficulty
{
    /// <summary>
    /// "난이도 → 스탯" 규칙의 집합.  기존 <c>DifficultyConfig</c> 의 상위 호환.
    ///
    /// 스탯마다 서로 다른 수식(<see cref="StatScalingSO"/>)을 매핑한다.
    ///   예) HP는 CurveScaling(완만한 3배), 이동속도는 LinearScaling(난이도당 +1%),
    ///       공격력은 ExponentialScaling(복리) ...
    ///
    /// 매핑되지 않은 스탯은 "스케일 안 함(기본값 유지)" 으로 처리된다.
    /// 예산(한 웨이브 몬스터 총량) 규칙도 여기서 함께 관리한다.
    ///
    /// 챕터/스테이지마다 다른 프로파일을 꽂을 수 있어(계층 override),
    /// "쉬운 챕터 / 어려운 챕터" 를 데이터만으로 갈아끼울 수 있다.
    /// </summary>
    [CreateAssetMenu(fileName = "DifficultyProfile", menuName = "Monster/Difficulty/Difficulty Profile")]
    public class DifficultyProfileSO : ScriptableObject
    {
        [System.Serializable]
        public struct StatRule
        {
            public StatType stat;
            public StatScalingSO scaling;   // 이 스탯에 적용할 수식
        }

        [Header("스탯별 스케일링 수식")]
        [SerializeField] private List<StatRule> _rules = new();


        [Header("난이도 배분 비율 (총 난이도를 채널로 나눔)")]
        [Tooltip("몬스터 수 채널 비중")]
        [SerializeField] private float _countRatio = 1f;
        [Tooltip("스탯(체력/이속) 채널 비중")]
        [SerializeField] private float _statRatio = 1f;
        [Tooltip("특성 채널 비중")]
        [SerializeField] private float _traitRatio = 1f;


        [Header("예산 (몬스터 수 채널 난이도 = 예산 포인트)")]
        [Tooltip("포인트 1당 배율. 보통 1. 몬스터 Cost 스케일이 난이도와 안 맞을 때만 조절")]
        [SerializeField] private float _budgetPerPoint = 1f;

        // 빠른 조회용 캐시 (스탯 → 수식). enum 인덱스로 바로 접근.
        private StatScalingSO[] _lookup;

        private void BuildLookup()
        {
            _lookup = new StatScalingSO[(int)StatType.Count];
            if (_rules == null) return;
            foreach (var r in _rules)
            {
                int i = (int)r.stat;
                if (i >= 0 && i < _lookup.Length) _lookup[i] = r.scaling;
            }
        }

        private void OnEnable() => BuildLookup();   // 로드/재컴파일 시 캐시 재구성

        /// <summary>해당 스탯의 스케일링 수식. 없으면 null(=스케일 안 함).</summary>
        public StatScalingSO GetScaling(StatType stat)
        {
            if (_lookup == null) BuildLookup();
            int i = (int)stat;
            return (i >= 0 && i < _lookup.Length) ? _lookup[i] : null;
        }

        /// <summary>
        /// 몬스터 수 채널 난이도(=예산 포인트) → 웨이브 예산.
        /// 별도 커브 없이 배정된 포인트를 그대로(×배율) 쓴다. Cost 는 이 포인트에서 차감.
        /// </summary>
        public int GetBudget(int countDifficulty)
            => Mathf.Max(1, Mathf.RoundToInt(countDifficulty * _budgetPerPoint));

        /// <summary>
        /// 총 난이도를 채널 비율대로 쪼갠다. (몬스터 수 / 스탯 / 특성)
        /// 반올림 오차는 마지막 채널(Trait)이 흡수해 합계가 총 난이도와 정확히 일치한다.
        /// </summary>
        public DifficultyAllocation Allocate(int totalDifficulty)
        {
            float sum = _countRatio + _statRatio + _traitRatio;
            if (sum <= 0f)   // 전부 0이면 균등 배분으로 폴백
            {
                int third = totalDifficulty / 3;
                return new DifficultyAllocation
                {
                    Total = totalDifficulty,
                    Count = third,
                    Stat = third,
                    Trait = totalDifficulty - third * 2,
                };
            }

            int count = Mathf.RoundToInt(totalDifficulty * (_countRatio / sum));
            int stat = Mathf.RoundToInt(totalDifficulty * (_statRatio / sum));
            count = Mathf.Clamp(count, 0, totalDifficulty);
            stat = Mathf.Clamp(stat, 0, totalDifficulty - count);
            int trait = totalDifficulty - count - stat;   // 나머지 = 오차 흡수

            return new DifficultyAllocation
            {
                Total = totalDifficulty,
                Count = count,
                Stat = stat,
                Trait = trait,
            };
        }
    }
}
