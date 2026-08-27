using System.Collections.Generic;
using Shield_Shot.GameplayCore.Monster.BT.Traits;
using Shield_Shot.GameplayCore.Monster.Movement;

namespace Shield_Shot.GameplayCore.Monster.Difficulty
{
    /// <summary>
    /// 한 "대형(formation)" 의 스폰 지시서. 같이 나오는 몬스터의 공통 청사진.
    ///
    /// [핵심] 종류(Unit)·스탯·이동·특성이 이 그룹 안에서 모두 동일하다.
    /// 구성기가 그룹당 한 번씩만 롤해서 채우므로, 같은 대형 몬스터는
    /// 반드시 같은 종류 + 같은 특성으로 스폰된다.
    /// </summary>
    public class MonsterGroupPlan
    {
        public MonsterUnitSO Unit;
        public int Count;                         // 이 대형의 마릿수
        public StatBlock Stats;                   // 난이도 적용된 최종 스탯 (그룹 공유)

        public readonly List<IMovementBehavior> Movements = new();  // 그룹 공유 이동 행동
        public readonly List<BtTraitSO> BehaviorTraits = new();     // 그룹 공유 생존 특성
        public readonly List<BtTraitSO> DeathTraits = new();        // 그룹 공유 사망 특성
    }

    /// <summary>
    /// 한 웨이브의 구성 결과. 기존 WavePlan 의 후계.
    ///
    /// 더 이상 단일 StatMultiplier 도, 개체 평면 리스트도 아니다.
    /// "대형(그룹)들의 목록" 으로 구성되고, 각 그룹이 스탯·특성을 자체 보유한다.
    /// </summary>
    public class DifficultyWavePlan
    {
        public int Difficulty;

        public readonly List<MonsterGroupPlan> Groups = new();   // 일반 몬스터 대형들

        public bool IsBoss;
        public MonsterGroupPlan BossGroup;                       // 보스(단일 대형, Count=1)

        /// <summary>전체 몬스터 마릿수(대형 합).</summary>
        public int TotalCount
        {
            get
            {
                int n = 0;
                foreach (var g in Groups) n += g.Count;
                return n;
            }
        }
    }
}
