using Shield_Shot.GameplayCore.Monster.BT.Core;
using Shield_Shot.GameplayCore.Monster.Core;
using Shield_Shot.GameplayCore.Monster.Movement;
using Shield_Shot.GameplayCore.Monster.Pool;
using Shield_Shot.GameplayCore.Monster.Stage;
using System.Collections.Generic;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.BT.Nodes
{
    public class SplitNode : BtNode<BtContext>
    {
        private readonly float _healthPercent;   // 부모 최대체력 대비 (0~1)
        private readonly int _splitCount;
        private readonly float _splitRadius;
        private readonly float _invincibilityDuration;

        private bool _split;
        private readonly List<IMovementBehavior> _behaviorBuf = new();

        public SplitNode(float healthPercent, int splitCount = 2, float splitRadius = 1f,
                         float invincibilityDuration = 0.5f)
        {
            _healthPercent = healthPercent;
            _splitCount = splitCount;
            _splitRadius = splitRadius;
            _invincibilityDuration = invincibilityDuration;
        }

        public override NodeState Execute(BtContext ctx)
        {
            if (ctx.Health.CurrentHealth > 0 || _split) return NodeState.Failure;
            if (ctx.Monster.SourcePrefab == null) return NodeState.Failure;   // _splitData 체크 제거

            _split = true;

            float childHp = ctx.Health.MaxHealth * _healthPercent;   // 부모 최대체력 × n%
            float childSpeed = ctx.Movement.BaseSpeed;                  // 부모 속도 그대로

            _behaviorBuf.Clear();
            foreach (var b in ctx.Monster.ActiveBehaviors)
                _behaviorBuf.Add(b);

            for (int i = 0; i < _splitCount; i++)
            {
                Vector3 offset = Random.insideUnitSphere * _splitRadius;
                offset.y = 0f;
                //var go = MonsterPoolManager.Instance.Get(
                //    ctx.Monster.SourcePrefab,
                //    ctx.Transform.position + offset,
                //    ctx.Transform.rotation);
                var go = MonsterFactory.Instance.Get(
                    ctx.Monster.SourcePrefab,
                    ctx.Transform.position + offset,
                    ctx.Transform.rotation);

                var child = go?.GetComponent<MonsterBase>();
                if (child != null)
                {
                    child.InitializeSimple(childHp, childSpeed, _behaviorBuf);
                    child.Health.StartInvincibility(_invincibilityDuration);
                    StageManager.Instance?.RegisterMonster(child);
                }
                else
                {
                    Debug.LogError($"[SplitNode] {ctx.Monster.SourcePrefab.name}에 MonsterBase 없음!");
                }
            }

            ctx.Despawn();
            return NodeState.Success;
        }

        public override void OnReset() => _split = false;
    }
}