using Shield_Shot.GameplayCore.Monster.BT.Core;

namespace Shield_Shot.GameplayCore.Monster.BT.Actions
{
    public class DieAction : ActionNode<BtContext>
    {
        public override NodeState Execute(BtContext ctx)
        {
            ctx.Despawn();
            return NodeState.Success;
        }
    }
}