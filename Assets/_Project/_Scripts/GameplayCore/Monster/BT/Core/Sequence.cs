namespace Shield_Shot.GameplayCore.Monster.BT.Core
{
    public class Sequence<T> : CompositeNode<T>
    {
        public Sequence(params BtNode<T>[] children) : base(children) { }

        public override NodeState Execute(T ctx)
        {
            foreach (var child in Children)
            {
                var result = child.Execute(ctx);
                if (result != NodeState.Success) return result;
            }
            return NodeState.Success;
        }
    }
}