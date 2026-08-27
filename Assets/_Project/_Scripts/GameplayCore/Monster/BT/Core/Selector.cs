namespace Shield_Shot.GameplayCore.Monster.BT.Core
{
    public class Selector<T> : CompositeNode<T>
    {
        public Selector(params BtNode<T>[] children) : base(children) { }

        public override NodeState Execute(T ctx)
        {
            foreach (var child in Children)
            {
                var result = child.Execute(ctx);
                if (result != NodeState.Failure) return result;
            }
            return NodeState.Failure;
        }
    }
}