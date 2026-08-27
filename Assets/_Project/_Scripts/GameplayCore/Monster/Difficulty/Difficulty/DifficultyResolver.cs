namespace Shield_Shot.GameplayCore.Monster.Difficulty
{
    /// <summary>
    /// 파이프라인의 심장.  (난이도 + 기본스탯 + 공통수식 + 몬스터성향) → 최종 StatBlock.
    ///
    /// MonoBehaviour 도 아니고 상태도 없다(순수 함수). 테스트가 쉽고 어디서든 호출 가능.
    ///
    /// 보간식:
    ///     final[stat] = base[stat] + (scaled[stat] - base[stat]) × growthWeight[stat]
    ///   - scaled = 난이도 프로파일의 수식 결과 (없으면 base)
    ///   - growthWeight = 몬스터 성향(없으면 1)
    ///
    /// 이 한 곳만 통과하면 모든 몬스터·보스가 동일 규칙으로 스탯을 받는다.
    /// "몬스터 SO에 스탯을 직접 박는" 방식이 사라지고, 난이도가 스탯을 '주입'한다.
    /// </summary>
    public static class DifficultyResolver
    {
        public static StatBlock Resolve(
            int difficulty,
            in StatBlock baseStats,
            DifficultyProfileSO profile,
            MonsterStatProfileSO monsterProfile = null)
        {
            StatBlock result = default;

            for (int i = 0; i < (int)StatType.Count; i++)
            {
                var stat = (StatType)i;
                float baseV = baseStats[stat];

                var scaling = profile != null ? profile.GetScaling(stat) : null;
                float scaledV = scaling != null ? scaling.Evaluate(baseV, difficulty) : baseV;

                float weight = monsterProfile != null ? monsterProfile.GetGrowthWeight(stat) : 1f;

                result[stat] = baseV + (scaledV - baseV) * weight;
            }

            return result;
        }
    }
}
