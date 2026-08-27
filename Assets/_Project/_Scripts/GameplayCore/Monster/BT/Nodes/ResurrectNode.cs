using Shield_Shot.GameplayCore.Monster.BT.Core;

namespace Shield_Shot.GameplayCore.Monster.BT.Nodes
{
    public class ResurrectNode : BtNode<BtContext>
    {
        private readonly int _maxRevives;
        private readonly float _reviveHealthPercent;
        private readonly float _stunDuration;
        private int _remaining;

        public ResurrectNode(int maxRevives, float reviveHealthPercent = 0.5f, float stunDuration = 2f)
        {
            _maxRevives = maxRevives;
            _reviveHealthPercent = reviveHealthPercent;
            _stunDuration = stunDuration;
            _remaining = maxRevives;
        }

        public override NodeState Execute(BtContext ctx)
        {
            if (ctx.Health.CurrentHealth > 0 || _remaining <= 0)
                return NodeState.Failure;

            _remaining--;
            ctx.Health.Initialize(ctx.Health.MaxHealth * _reviveHealthPercent);
            ctx.Health.StartInvincibility(_stunDuration);
            ctx.SetCollider(true);
            ctx.Movement.StartStun(_stunDuration);   // ← enabled 유지, 내부에서 멈춤
            return NodeState.Success;
        }

        public override void OnReset() => _remaining = _maxRevives;

    }
}
