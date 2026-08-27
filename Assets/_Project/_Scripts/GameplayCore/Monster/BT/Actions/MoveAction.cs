using Shield_Shot.GameplayCore.Monster.BT.Core;

namespace Shield_Shot.GameplayCore.Monster.BT.Actions
{
    public class MoveAction : ActionNode<BtContext>
    {
        public override NodeState Execute(BtContext ctx)
        {
            if (!ctx.Movement.enabled)
                ctx.Movement.enabled = true;
            return NodeState.Running;
        }
    }
}