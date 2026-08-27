using Shield_Shot.GameplayCore.Monster.BT.Core;

namespace Shield_Shot.GameplayCore.Monster.BT.Conditions
{
    public class IsAliveCondition : ConditionNode<BtContext>
    {
        protected override bool Evaluate(BtContext ctx)
            => ctx.Health.CurrentHealth > 0;
    }
}