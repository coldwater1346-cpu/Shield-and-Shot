using Shield_Shot.GameplayCore.Monster.BT.Core;
using Shield_Shot.GameplayCore.Monster.Core;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.BT.Nodes
{
    public class DeadAnimNode : BtNode<BtContext>
    {
        private readonly float _waitDuration;
        private float _timer;
        private bool _triggered;

        public DeadAnimNode(float waitDuration = 1f)
        {
            _waitDuration = waitDuration;
        }

        public override NodeState Execute(BtContext ctx)
        {
            if (!_triggered)
            {
                Debug.Log($"[DeadAnim] {ctx.Monster.name} 사망 애니 시작");
                ctx.Movement.Stop();
                ctx.Movement.enabled = false;
                ctx.SetCollider(false);

                if (ctx.Anim == null) return NodeState.Success;
                ctx.Anim.TriggerDead();
                _triggered = true;
                _timer = _waitDuration;
            }

            _timer -= Time.deltaTime;
            return _timer > 0f ? NodeState.Running : NodeState.Success;
        }

        public override void OnReset()
        {
            _triggered = false;
            _timer = 0f;
        }
    }
}