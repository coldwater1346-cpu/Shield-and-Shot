using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.Difficulty
{
    /// <summary>
    /// 스테이지 하나 = 웨이브 목록.  기존 StoryStageSO 의 후계이자 "챕터의 부품".
    ///
    /// 스스로도 <see cref="IDifficultyWaveSource"/> 라 단독 실행이 가능하고(테스트/미니 스테이지),
    /// 챕터에 담기면 챕터가 override 를 얹어 이어붙인다.
    ///
    /// 풀/프로파일/특성규칙/그룹크기를 비워두면(null·0) 상위(챕터)의 기본값을 쓴다 → 계층 override.
    /// </summary>
    [CreateAssetMenu(fileName = "StageDefinition", menuName = "Monster/Difficulty/Stage Definition")]
    public class StageDefinitionSO : ScriptableObject, IDifficultyWaveSource
    {
        [Serializable]
        public struct WaveDef
        {
            [Tooltip("이 웨이브의 난이도 수치")] public int difficulty;
            [Tooltip("보스 웨이브 여부")] public bool isBoss;
        }

        [Header("이 스테이지 전용 override (비우면 챕터 기본값 사용)")]
        [SerializeField] private MonsterUnitPoolSO _poolOverride;
        [SerializeField] private DifficultyProfileSO _profileOverride;
        [SerializeField] private DifficultyProfileSO _bossProfileOverride;
        [SerializeField] private TraitRuleSetSO _traitRulesOverride;

        [Header("대형 크기 override (사용 체크 시에만 적용)")]
        [SerializeField] private bool _overrideGroupSize = false;
        [SerializeField] private int _groupSizeMin = 1;
        [SerializeField] private int _groupSizeMax = 5;

        [Header("웨이브")]
        [SerializeField] private List<WaveDef> _waves = new();

        [Header("보상")]
        [SerializeField] private int _rewardGold = 100;
        public int RewardGold => _rewardGold;

        public IReadOnlyList<WaveDef> Waves => _waves;
        public MonsterUnitPoolSO PoolOverride => _poolOverride;
        public DifficultyProfileSO ProfileOverride => _profileOverride;
        public DifficultyProfileSO BossProfileOverride => _bossProfileOverride;
        public TraitRuleSetSO TraitRulesOverride => _traitRulesOverride;
        public bool OverrideGroupSize => _overrideGroupSize;
        public int GroupSizeMin => _groupSizeMin;
        public int GroupSizeMax => _groupSizeMax;

        // ── IDifficultyWaveSource (단독 실행용. 챕터는 자체 로직으로 해석) ──
        public int WaveCount => _waves.Count;

        public bool TryGetWave(int waveIndex, out ResolvedWave wave)
        {
            if (waveIndex < 0 || waveIndex >= _waves.Count) { wave = default; return false; }
            var w = _waves[waveIndex];
            wave = new ResolvedWave
            {
                difficulty = w.difficulty,
                isBoss = w.isBoss,
                pool = _poolOverride,
                profile = w.isBoss ? (_bossProfileOverride != null ? _bossProfileOverride : _profileOverride)
                                   : _profileOverride,
                traitRules = _traitRulesOverride,
                minGroupSize = _overrideGroupSize ? _groupSizeMin : 1,
                maxGroupSize = _overrideGroupSize ? _groupSizeMax : 5,
                stageIndex = 0,
            };
            return true;
        }

        public IEnumerable<MonsterUnitPoolSO> EnumeratePools()
        {
            if (_poolOverride != null) yield return _poolOverride;
        }
    }
}
