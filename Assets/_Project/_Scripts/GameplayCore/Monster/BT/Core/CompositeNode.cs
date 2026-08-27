using System.Collections.Generic;

namespace Shield_Shot.GameplayCore.Monster.BT.Core
{
    public abstract class CompositeNode<T> : BtNode<T>
    {
        protected readonly List<BtNode<T>> Children;

        protected CompositeNode(params BtNode<T>[] children)
            => Children = new List<BtNode<T>>(children);

        public override void OnReset()
        {
            foreach (var child in Children)
                child.OnReset();
        }
    }
}