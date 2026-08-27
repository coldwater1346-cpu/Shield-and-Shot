using Shield_Shot.GameplayCore.Monster.BT.Core;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.BT.Nodes
{
    public class KnockbackNode : BtNode<BtContext>
    {
        private readonly float _knockbackDistance;
        private readonly float _knockbackDuration;

        private bool _used;

        public KnockbackNode(float knockbackDistance = 1f, float knockbackDuration = 0.1f)
        {
            _knockbackDistance = knockbackDistance;
            _knockbackDuration = knockbackDuration;
        }

        public override NodeState Execute(BtContext ctx)
        {
            if (ctx.Health.CurrentHealth > 0 || _used) return NodeState.Failure;

            _used = true;
            ctx.Health.Initialize(ctx.Health.MaxHealth);
            ctx.Movement.ApplyKnockback(
                ctx.Transform.forward * -_knockbackDistance,
                _knockbackDuration);

            return NodeState.Success;
        }

        public override void OnReset() => _used = false;
    }
}