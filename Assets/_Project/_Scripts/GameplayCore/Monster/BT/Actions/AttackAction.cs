using Shield_Shot.GameplayCore.Monster.BT.Core;

namespace Shield_Shot.GameplayCore.Monster.BT.Actions
{
    public class AttackAction : ActionNode<BtContext>
    {
        public override NodeState Execute(BtContext ctx)
        {
            ctx.Attack.TryExecuteRandom();
            return NodeState.Success;
        }
    }
}