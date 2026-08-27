using System.Collections.Generic;

namespace Shield_Shot.GameplayCore.Monster.Difficulty
{
    /// <summary>
    /// 웨이브 하나를 구성기(<see cref="DifficultyWaveComposer"/>)에 넘길 때 필요한 모든 컨텍스트.
    /// 계층(캠페인→챕터→스테이지→웨이브)에서 override 가 모두 해석된 "최종본".
    /// </summary>
    public struct ResolvedWave
    {
        public int difficulty;
        public bool isBoss;
        public MonsterUnitPoolSO pool;      // 이 웨이브에 쓸 몬스터 풀
        public DifficultyProfileSO profile; // 이 웨이브에 쓸 스탯 수식 묶음
        public TraitRuleSetSO traitRules;   // 이 웨이브에서 주입할 특성 규칙 (없으면 특성 없음)

        // 대형(그룹) 크기: 한 번에 같이 스폰되는 동종 몬스터 수 범위
        public int minGroupSize;
        public int maxGroupSize;

        public int stageIndex;
        public MonsterUnitPoolSO.UnitGroup group;
    }

    /// <summary>
    /// 웨이브 시퀀스를 제공하는 것들의 공통 인터페이스.
    /// StoryStage/Chapter/무한모드가 모두 이걸 구현하면 StageManager 는 하나의 흐름만 안다.
    /// </summary>
    public interface IDifficultyWaveSource
    {
        /// <summary>웨이브 총 개수. 무한 모드는 int.MaxValue.</summary>
        int WaveCount { get; }

        /// <summary>waveIndex 의 웨이브 컨텍스트를 해석해 반환. 끝이면 false.</summary>
        bool TryGetWave(int waveIndex, out ResolvedWave wave);

        /// <summary>프리워밍/검증용: 이 소스가 쓸 수 있는 모든 풀.</summary>
        IEnumerable<MonsterUnitPoolSO> EnumeratePools();
    }
}
