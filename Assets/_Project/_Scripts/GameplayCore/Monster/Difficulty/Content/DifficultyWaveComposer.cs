using System.Collections.Generic;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.Difficulty
{
    /// <summary>
    /// 난이도 컨텍스트를 받아 웨이브를 "대형(그룹) 단위" 로 구성한다. 기존 WaveComposer 의 후계.
    ///
    /// 총 난이도는 먼저 채널(수량/스탯/특성)로 비율 배분된 뒤 각 파트에 쓰인다:
    ///   alloc.Count → 예산(마릿수),  alloc.Stat → 스탯 스케일,  alloc.Trait → 특성.
    /// 단, 몬스터 해금(minDifficulty)·보스 선택은 총 난이도(ctx.difficulty) 로 판단한다.
    ///
    /// 그룹 하나 = 같은 종류 몬스터 여러 마리. 그룹을 만들 때 스탯/이동/특성을
    /// '한 번만' 굴려 그룹 전원이 공유하게 한다 → 대형은 항상 동종·동특성.
    /// 상태가 없어 여러 스테이지에서 재사용 가능(버퍼만 내부 보유).
    /// </summary>
    public class DifficultyWaveComposer
    {
        private readonly List<MonsterUnitSO> _candidateBuf = new();

        public DifficultyWavePlan Compose(in ResolvedWave ctx)
        {
            var plan = new DifficultyWavePlan { Difficulty = ctx.difficulty };

            var Group = ctx.group;
            if (Group == null) return plan;

            // 총 난이도 → 채널별 배분 (수량 / 스탯 / 특성)
            DifficultyAllocation alloc = ctx.profile != null
                ? ctx.profile.Allocate(ctx.difficulty)
                : new DifficultyAllocation { Total = ctx.difficulty, Stat = ctx.difficulty, Trait = ctx.difficulty };

            int minG = Mathf.Max(1, ctx.minGroupSize);
            int maxG = Mathf.Max(minG, ctx.maxGroupSize);

            // 예산은 '수량 채널' 난이도로 계산
            int budget = ctx.profile != null ? ctx.profile.GetBudget(alloc.Count) : 0;

            // ── 예산 소진까지 '대형' 단위로 채운다 ──────────────
            int safety = 1000;
            while (budget > 0 && safety-- > 0)
            {
                // 해금/후보 판단은 총 난이도로
                Group.GetCandidates(ctx.difficulty, budget, _candidateBuf);
                if (_candidateBuf.Count == 0) break;

                var unit = WeightedPick(_candidateBuf);

                int affordable = Mathf.Max(1, budget / unit.Cost);
                int size = Mathf.Clamp(Random.Range(minG, maxG + 1), 1, affordable);

                budget -= unit.Cost * size;
                plan.Groups.Add(BuildGroup(unit, size, ctx, alloc));
            }

            // 한 그룹도 못 만들었으면 최소 1마리 보장 (보스 웨이브 제외)
            if (!ctx.isBoss && plan.Groups.Count == 0)
            {
                Group.GetCandidates(ctx.difficulty, int.MaxValue, _candidateBuf);
                var cheapest = Cheapest(_candidateBuf);
                if (cheapest != null) plan.Groups.Add(BuildGroup(cheapest, 1, ctx, alloc));
            }

            // ── 보스 웨이브 ───────────────────────────────
            if (ctx.isBoss)
            {
                var boss = Group.PickBoss(ctx.difficulty);   // 보스 선택도 총 난이도
                if (boss != null)
                {
                    plan.IsBoss = true;
                    plan.BossGroup = BuildGroup(boss, 1, ctx, alloc);
                }
            }

            return plan;
        }

        /// <summary>
        /// 그룹 하나를 만든다. 스탯은 Stat 채널, 특성은 Trait 채널 난이도로 딱 한 번 굴려 그룹 전원이 공유.
        /// </summary>
        private static MonsterGroupPlan BuildGroup(
            MonsterUnitSO unit, int count, in ResolvedWave ctx, in DifficultyAllocation alloc)
        {
            var group = new MonsterGroupPlan { Unit = unit, Count = count };

            group.Stats = DifficultyResolver.Resolve(
                alloc.Stat, unit.BaseStats, ctx.profile, unit.StatProfile);   // 스탯 채널

            unit.GetMovements(group.Movements);   // 그룹 공유 이동(1회 롤)
            TraitInjector.Resolve(               // 특성 채널로 그룹 공유 특성(1회 롤)
                unit, alloc.Trait, ctx.traitRules, group.BehaviorTraits, group.DeathTraits);

            return group;
        }

        private static MonsterUnitSO WeightedPick(List<MonsterUnitSO> candidates)
        {
            float total = 0f;
            foreach (var c in candidates) total += c.Weight;
            if (total <= 0f) return candidates[Random.Range(0, candidates.Count)];

            float r = Random.Range(0f, total);
            float cum = 0f;
            foreach (var c in candidates)
            {
                cum += c.Weight;
                if (r <= cum) return c;
            }
            return candidates[candidates.Count - 1];
        }

        private static MonsterUnitSO Cheapest(List<MonsterUnitSO> candidates)
        {
            MonsterUnitSO best = null;
            foreach (var c in candidates)
                if (best == null || c.Cost < best.Cost) best = c;
            return best;
        }
    }
}