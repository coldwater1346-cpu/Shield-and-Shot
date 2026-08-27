using System.Collections.Generic;

namespace Shield_Shot.GameplayCore.Monster.Difficulty
{
    /// <summary>챕터 안의 스테이지 "하나"만 재생하는 런타임 소스 (챕터 공용 설정 적용).</summary>
    public class ChapterStageSource : IDifficultyWaveSource, IStageReward
    {
        private readonly ChapterSO _chapter;
        private readonly int _stageOrdinal;

        public ChapterStageSource(ChapterSO chapter, int stageOrdinal)
        {
            _chapter = chapter;
            _stageOrdinal = stageOrdinal;
        }

        public int WaveCount
        {
            get { var s = _chapter != null ? _chapter.GetStage(_stageOrdinal) : null; return s != null ? s.WaveCount : 0; }
        }

        public bool TryGetWave(int localWaveIndex, out ResolvedWave wave)
        {
            if (_chapter == null) { wave = default; return false; }
            return _chapter.ResolveStageWave(_stageOrdinal, localWaveIndex, out wave);
        }

        public IEnumerable<MonsterUnitPoolSO> EnumeratePools()
        {
            var p = _chapter != null ? _chapter.GetStagePool(_stageOrdinal) : null;
            if (p != null) yield return p;
        }

        public int RewardGold
        {
            get
            {
                var s = _chapter != null ? _chapter.GetStage(_stageOrdinal) : null;
                return s != null ? s.RewardGold : 0;
            }
        }
    }
}