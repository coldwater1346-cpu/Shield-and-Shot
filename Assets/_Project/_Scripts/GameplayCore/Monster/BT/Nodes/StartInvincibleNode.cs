using Shield_Shot.GameplayCore.Monster.BT.Core;

namespace Shield_Shot.GameplayCore.Monster.BT.Nodes
{
    // 즉시 무적 시작 (깜박임 트리거용). 바로 Success.
    public class StartInvincibleNode : BtNode<BtContext>
    {
        private readonly float _duration;
        public StartInvincibleNode(float duration) => _duration = duration;

        public override NodeState Execute(BtContext ctx)
        {
            ctx.Health.StartInvincibility(_duration);
            return NodeState.Success;
        }
    }
}