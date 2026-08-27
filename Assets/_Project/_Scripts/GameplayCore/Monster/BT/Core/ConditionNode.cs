

namespace Shield_Shot.GameplayCore.Monster.BT.Core
{
    public abstract class ConditionNode<T> : BtNode<T>
    {
        public override NodeState Execute(T ctx)
            => Evaluate(ctx) ? NodeState.Success : NodeState.Failure;

        protected abstract bool Evaluate(T ctx);
    }
}