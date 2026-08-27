using Shield_Shot.GameplayCore.Monster.BT.Core;

namespace Shield_Shot.GameplayCore.Monster.BT.Nodes
{
    // 이동 정지 + 콜라이더 해제. 사망/부활/분열 진입의 공통 첫 단계.
    public class DisableCombatNode : BtNode<BtContext>
    {
        public override NodeState Execute(BtContext ctx)
        {
            ctx.Movement.Stop();      // Stop() 내부에서 enabled=false 처리
            ctx.SetCollider(false);
            return NodeState.Success;
        }
    }
}