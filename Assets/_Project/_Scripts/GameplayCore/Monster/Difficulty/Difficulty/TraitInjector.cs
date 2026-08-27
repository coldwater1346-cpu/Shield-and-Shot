using System.Collections.Generic;
using UnityEngine;
using Shield_Shot.GameplayCore.Monster.BT.Traits;

namespace Shield_Shot.GameplayCore.Monster.Difficulty
{
    /// <summary>
    /// (몬스터 태그 + 난이도 + 규칙셋) → 실제 부여할 특성 목록.  스탯의 DifficultyResolver 에 대응하는 "특성판".
    ///
    /// [중요] 확률 롤은 여기서 딱 한 번 일어난다. 구성기가 이걸 '그룹당 1회' 호출하므로,
    /// 한 대형으로 같이 스폰되는 몬스터들은 같은 종류 + 같은 특성을 공유하게 된다.
    ///
    /// 우선순위: 몬스터 고유 innate 특성(항상) → 규칙셋에서 롤로 추가.
    /// </summary>
    public static class TraitInjector
    {
        /// <summary>
        /// 한 그룹의 특성을 굴린다. behaviorOut/deathOut 은 호출 측이 재사용하는 버퍼.
        /// </summary>
        public static void Resolve(
            MonsterUnitSO unit,
            int difficulty,
            TraitRuleSetSO ruleSet,
            List<BtTraitSO> behaviorOut,
            List<BtTraitSO> deathOut)
        {
            behaviorOut.Clear();
            deathOut.Clear();

            // 1) 몬스터 고유(innate) 특성은 항상 포함 (있으면)
            unit.CollectInnateBehaviorTraits(behaviorOut);
            unit.CollectInnateDeathTraits(deathOut);

            // 2) 규칙셋에서 확률 주입
            if (ruleSet == null) return;

            MonsterTag tags = unit.Tags;
            int maxB = ruleSet.MaxBehaviorTraits;
            int maxD = ruleSet.MaxDeathTraits;

            var rules = ruleSet.Rules;
            for (int i = 0; i < rules.Count; i++)
            {
                var rule = rules[i];
                if (!rule.Matches(difficulty, tags)) continue;
                if (Random.value > rule.chance) continue;

                if (rule.slot == TraitSlot.Behavior)
                {
                    if (maxB > 0 && behaviorOut.Count >= maxB) continue;
                    if (!behaviorOut.Contains(rule.trait)) behaviorOut.Add(rule.trait);
                }
                else // Death
                {
                    if (maxD > 0 && deathOut.Count >= maxD) continue;
                    if (!deathOut.Contains(rule.trait)) deathOut.Add(rule.trait);
                }
            }
        }
    }
}
