using System.Collections.Generic;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.Difficulty
{
    /// <summary>
    /// 챕터 = 스테이지 모음 + 공통 기본값. 이제 "스테이지 하나만" 재생하는 소스도 만들 수 있다.
    /// override 우선순위: Stage 개별 override → Chapter 기본값.
    /// </summary>
    [CreateAssetMenu(fileName = "Chapter", menuName = "Monster/Difficulty/Chapter")]
    public class ChapterSO : ScriptableObject, IDifficultyWaveSource
    {
        [Header("메타")]
        [SerializeField] private string _displayName = "Chapter";

        [Header("바이옴")]
        [SerializeField] private ChapterBiom _biom = ChapterBiom.None;

        [Header("챕터 공통 기본값 (스테이지가 비우면 사용)")]
        [SerializeField] private MonsterUnitPoolSO _defaultPool;
        [SerializeField] private DifficultyProfileSO _defaultProfile;
        [SerializeField] private DifficultyProfileSO _bossProfile;
        [SerializeField] private TraitRuleSetSO _defaultTraitRules;

        [Tooltip("이 챕터의 모든 웨이브 난이도에 더해지는 값")]
        [SerializeField] private int _difficultyOffset = 0;

        [Header("대형 크기 기본값")]
        [SerializeField] private int _groupSizeMin = 1;
        [SerializeField] private int _groupSizeMax = 5;

        [Header("스테이지 목록")]
        [SerializeField] private List<StageDefinitionSO> _stages = new();

   

        public ChapterBiom Biom => _biom;
        public string DisplayName => _displayName;
        public int StageCount => _stages.Count;

        public StageDefinitionSO GetStage(int ordinal)
            => (ordinal >= 0 && ordinal < _stages.Count) ? _stages[ordinal] : null;

        /// <summary>이 스테이지에 쓸 풀(스테이지 override → 챕터 기본). 프리워밍용.</summary>
        public MonsterUnitPoolSO GetStagePool(int stageOrdinal)
        {
            var s = GetStage(stageOrdinal);
            if (s == null) return _defaultPool;
            return s.PoolOverride != null ? s.PoolOverride : _defaultPool;
        }

        /// <summary>단일 스테이지를 챕터 공용 설정과 함께 재생하는 소스.</summary>
        public IDifficultyWaveSource CreateStageSource(int stageOrdinal)
            => new ChapterStageSource(this, stageOrdinal);

        /// <summary>스테이지 한 웨이브를 챕터 override 규칙으로 해석.</summary>
        public bool ResolveStageWave(int stageOrdinal, int localIndex, out ResolvedWave wave)
        {
            wave = default;
            var stage = GetStage(stageOrdinal);
            if (stage == null || localIndex < 0 || localIndex >= stage.WaveCount) return false;

            var def = stage.Waves[localIndex];
            var pool = stage.PoolOverride != null ? stage.PoolOverride : _defaultPool;

            DifficultyProfileSO profile;
            if (def.isBoss)
                profile = stage.BossProfileOverride != null ? stage.BossProfileOverride
                        : _bossProfile != null ? _bossProfile
                        : stage.ProfileOverride != null ? stage.ProfileOverride
                        : _defaultProfile;
            else
                profile = stage.ProfileOverride != null ? stage.ProfileOverride : _defaultProfile;

            var traitRules = stage.TraitRulesOverride != null ? stage.TraitRulesOverride : _defaultTraitRules;
            int gMin = stage.OverrideGroupSize ? stage.GroupSizeMin : _groupSizeMin;
            int gMax = stage.OverrideGroupSize ? stage.GroupSizeMax : _groupSizeMax;

            wave = new ResolvedWave
            {
                difficulty = def.difficulty + _difficultyOffset,
                isBoss = def.isBoss,
                pool = pool,
                profile = profile,
                traitRules = traitRules,
                minGroupSize = gMin,
                maxGroupSize = gMax,
                stageIndex = stageOrdinal,
            };
            return true;
        }

        // ── (선택) 챕터 전체를 이어서 재생하고 싶을 때용. B 방식에선 안 씀 ──
        private readonly List<(int stageOrdinal, int localIndex)> _flat = new();
        private bool _flatBuilt;
        private void OnEnable() { _flatBuilt = false; }
        private void BuildFlat()
        {
            _flat.Clear();
            for (int s = 0; s < _stages.Count; s++)
            {
                var stage = _stages[s];
                if (stage == null) continue;
                for (int i = 0; i < stage.WaveCount; i++) _flat.Add((s, i));
            }
            _flatBuilt = true;
        }
        public int WaveCount { get { if (!_flatBuilt) BuildFlat(); return _flat.Count; } }
        public bool TryGetWave(int waveIndex, out ResolvedWave wave)
        {
            if (!_flatBuilt) BuildFlat();
            if (waveIndex < 0 || waveIndex >= _flat.Count) { wave = default; return false; }
            var (ordinal, local) = _flat[waveIndex];
            return ResolveStageWave(ordinal, local, out wave);
        }

        public IEnumerable<MonsterUnitPoolSO> EnumeratePools()
        {
            var seen = new HashSet<MonsterUnitPoolSO>();
            if (_defaultPool != null && seen.Add(_defaultPool)) yield return _defaultPool;
            foreach (var s in _stages)
            {
                if (s == null) continue;
                foreach (var p in s.EnumeratePools())
                    if (p != null && seen.Add(p)) yield return p;
            }
        }
    }
}