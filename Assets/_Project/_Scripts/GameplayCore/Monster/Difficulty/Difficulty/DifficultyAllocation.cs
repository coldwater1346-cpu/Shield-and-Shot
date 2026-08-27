namespace Shield_Shot.GameplayCore.Monster.Difficulty
{
    /// <summary>
    /// 난이도를 나눠 담는 "채널". 총 난이도를 이 채널들로 비율 배분한다.
    ///
    /// 채널을 추가하려면: 여기에 항목 1개 + DifficultyAllocation 에 필드+case 1개.
    /// (예: 공격속도 예산, 스폰 빈도 예산 등)
    /// </summary>
    public enum DifficultyChannel
    {
        Count,   // 몬스터 수(Cost 예산)
        Stat,    // 체력/이속 등 스탯 배율
        Trait,   // 특성 부여(해금·확률)
    }

    /// <summary>
    /// 총 난이도를 채널별 하위 난이도로 쪼갠 결과.
    ///   총 50, 비율 count:stat:trait = 2:2:1 → count 20 / stat 20 / trait 10.
    ///
    /// 각 채널은 자기 몫의 난이도만 보고 동작한다:
    ///   Count → 예산(마릿수),  Stat → 스탯 스케일,  Trait → 특성 게이트/확률.
    /// 몬스터 해금(minDifficulty)·보스 선택은 채널이 아니라 Total 로 판단한다.
    /// </summary>
    public struct DifficultyAllocation
    {
        public int Total;
        public int Count;
        public int Stat;
        public int Trait;

        public int this[DifficultyChannel ch]
        {
            get
            {
                switch (ch)
                {
                    case DifficultyChannel.Count: return Count;
                    case DifficultyChannel.Stat: return Stat;
                    case DifficultyChannel.Trait: return Trait;
                    default: return 0;
                }
            }
        }

        public override string ToString()
            => $"[diff {Total} → count {Count} / stat {Stat} / trait {Trait}]";
    }
}