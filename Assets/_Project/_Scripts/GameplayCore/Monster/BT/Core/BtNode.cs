namespace Shield_Shot.GameplayCore.Monster.BT.Core
{
    public abstract class BtNode<T>
    {
        public abstract NodeState Execute(T ctx);
        public virtual void OnReset() { }
    }
}