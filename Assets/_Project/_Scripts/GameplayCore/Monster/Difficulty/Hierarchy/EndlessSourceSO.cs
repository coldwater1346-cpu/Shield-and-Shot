using System.Collections.Generic;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.Difficulty
{
    /// <summary>
    /// 무한 모드 소스. 기존 InfiniteModeSO/InfiniteWaveSource 의 후계.
    ///
    /// 웨이브가 끝없이 이어지며 난이도가 계속 오른다. 같은 IDifficultyWaveSource 라
    /// StageManager 는 스토리/챕터/무한을 구분 없이 동일 흐름으로 돌린다.
    /// </summary>
    [CreateAssetMenu(fileName = "EndlessSource", menuName = "Monster/Difficulty/Endless Source")]
    public class EndlessSourceSO : ScriptableObject, IDifficultyWaveSource
    {
        [SerializeField] private MonsterUnitPoolSO _pool;
        [SerializeField] private DifficultyProfileSO _profile;
        [SerializeField] private DifficultyProfileSO _bossProfile;   // 선택
        [SerializeField] private TraitRuleSetSO _traitRules;         // 무한 모드 특성 규칙

        [Header("대형 크기")]
        [SerializeField] private int _groupSizeMin = 1;
        [SerializeField] private int _groupSizeMax = 5;

        [Header("난이도 증가")]
        [Tooltip("시작 난이도 (장비 점수 등으로 대체 가능)")]
        [SerializeField] private int _baseDifficulty = 10;
        [Tooltip("웨이브마다 더해지는 난이도")]
        [SerializeField] private int _incrementPerWave = 5;
        [Tooltip("N 웨이브마다 보스")]
        [SerializeField] private int _bossEveryNWaves = 5;

        /// <summary>외부(장비 점수 등)에서 시작 난이도를 주입할 때 사용.</summary>
        public int StartDifficultyOverride { get; set; } = -1;

        public int WaveCount => int.MaxValue;   // 무한

        public bool TryGetWave(int waveIndex, out ResolvedWave wave)
        {
            int start = StartDifficultyOverride >= 0 ? StartDifficultyOverride : _baseDifficulty;
            bool isBoss = _bossEveryNWaves > 0 && ((waveIndex + 1) % _bossEveryNWaves == 0);

            wave = new ResolvedWave
            {
                difficulty = start + _incrementPerWave * waveIndex,
                isBoss = isBoss,
                pool = _pool,
                profile = isBoss && _bossProfile != null ? _bossProfile : _profile,
                traitRules = _traitRules,
                minGroupSize = _groupSizeMin,
                maxGroupSize = _groupSizeMax,
                stageIndex = 0,  // 무한 모드에는 스테이지가 없음
            };
            return true;   // 끝나지 않음
        }

        public IEnumerable<MonsterUnitPoolSO> EnumeratePools()
        {
            if (_pool != null) yield return _pool;
        }
    }
}
