using System.Collections.Generic;
using Shield_Shot.GameplayCore.Monster.BT;
using Shield_Shot.GameplayCore.Monster.BT.Actions;
using Shield_Shot.GameplayCore.Monster.BT.Conditions;
using Shield_Shot.GameplayCore.Monster.BT.Core;
using Shield_Shot.GameplayCore.Monster.BT.Nodes;
using Shield_Shot.GameplayCore.Monster.BT.Traits;
using Shield_Shot.GameplayCore.Render;

namespace Shield_Shot.GameplayCore.Monster.Core
{
    /// <summary>몬스터 행동트리 조립 전담. MonsterBase는 생명주기·컴포넌트 조립만 담당한다.</summary>
    public static class MonsterBtFactory
    {
        // 일반 몬스터: 특성 행동 → 공격 → 이동
        public static BtNode<BtContext> BuildNormal(
            List<BtTraitSO> behaviorTraits, List<BtTraitSO> deathTraits, VFXType deathVFX)
        {
            var behaviorNodes = new List<BtNode<BtContext>>();
            AddTraitNodes(behaviorTraits, behaviorNodes);
            behaviorNodes.Add(new Sequence<BtContext>(new AttackReadyCondition(), new AttackAction()));
            behaviorNodes.Add(new MoveAction());

            return BuildRoot(new Selector<BtContext>(behaviorNodes.ToArray()), deathTraits, deathVFX);
        }

        // 보스: 전용 공격 노드만
        public static BtNode<BtContext> BuildBoss(
            List<BtTraitSO> deathTraits, float fireInterval, VFXType deathVFX)
            => BuildRoot(new BossAttackNode(fireInterval), deathTraits, deathVFX);

        // 분열 자식 등 단순 개체: 이동만
        public static BtNode<BtContext> BuildSimple(VFXType deathVFX)
            => BuildRoot(new MoveAction(), null, deathVFX);

        // ── 공통 조립 ─────────────────────────────────────
        private static BtNode<BtContext> BuildRoot(
            BtNode<BtContext> aliveBranch, List<BtTraitSO> deathTraits, VFXType deathVFX)
            => new Selector<BtContext>(
                new Sequence<BtContext>(new IsAliveCondition(), aliveBranch),
                BuildDeathSelector(deathTraits, deathVFX));

        private static BtNode<BtContext> BuildDeathSelector(List<BtTraitSO> deathTraits, VFXType deathVFX)
        {
            var deathNodes = new List<BtNode<BtContext>>();
            AddTraitNodes(deathTraits, deathNodes);
            deathNodes.Add(DefaultDeath(deathVFX));   // 사망 특성이 없거나 실패하면 기본 사망
            return new Selector<BtContext>(deathNodes.ToArray());
        }

        private static BtNode<BtContext> DefaultDeath(VFXType deathVFX)
            => new Sequence<BtContext>(
                new DisableCombatNode(),
                new AnimTriggerNode("Dead"),   // 기존 DeadAnimNode의 Dead 트리거
                new WaitNode(1f),              // 기존 DeadAnimNode의 1초 대기
                new SpawnVfxNode(deathVFX),    // 기존 DeathReactNode의 VFX
                new DieAction());

        private static void AddTraitNodes(List<BtTraitSO> traits, List<BtNode<BtContext>> outList)
        {
            if (traits == null) return;
            for (int i = 0; i < traits.Count; i++)
                if (traits[i] != null) outList.Add(traits[i].CreateNode());
        }
    }
}