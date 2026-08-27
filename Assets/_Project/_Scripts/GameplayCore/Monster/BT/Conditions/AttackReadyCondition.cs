using Shield_Shot.GameplayCore.Monster.BT.Core;

namespace Shield_Shot.GameplayCore.Monster.BT.Conditions
{
    public class AttackReadyCondition : ConditionNode<BtContext>
    {
        protected override bool Evaluate(BtContext ctx)
            => ctx.Attack != null && ctx.Attack.IsReady;
    }
}