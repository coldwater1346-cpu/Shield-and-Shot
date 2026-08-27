using Shield_Shot.GameplayCore.Monster.BT.Core;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.BT.Nodes
{
    public class BossAttackNode : BtNode<BtContext>
    {
        private readonly float _fireInterval;
        private float _timer;

        public BossAttackNode(float fireInterval)
        {
            _fireInterval = fireInterval;
            _timer = fireInterval;
        }

        public override NodeState Execute(BtContext ctx)
        {
            if (!ctx.Attack.IsReady) return NodeState.Running; // 공격 실행 중 - 타이머 정지

            _timer -= Time.deltaTime;
            if (_timer > 0f) return NodeState.Running;        // 대기 중

            _timer = _fireInterval;
            ctx.Attack.TryExecuteRandom();
            return NodeState.Running;
        }

        public override void OnReset() => _timer = _fireInterval;
    }
}