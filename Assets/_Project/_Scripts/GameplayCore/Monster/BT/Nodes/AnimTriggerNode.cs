using Shield_Shot.GameplayCore.Monster.BT.Core;
using Shield_Shot.GameplayCore.Monster.Core;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.BT.Nodes
{
    public class AnimTriggerNode : BtNode<BtContext>
    {
        private readonly int _triggerHash;
        private readonly float _hold;   // 애니 재생 대기 시간(0이면 즉시 통과)
        private float _timer;
        private bool _started;

        public AnimTriggerNode(string trigger, float hold = 0f)
        {
            _triggerHash = Animator.StringToHash(trigger);
            _hold = hold;
        }

        public override NodeState Execute(BtContext ctx)
        {
            if (!_started)
            {
                ctx.Anim?.PlayTrigger(_triggerHash);
                _timer = _hold;
                _started = true;
            }

            _timer -= Time.deltaTime;
            return _timer > 0f ? NodeState.Running : NodeState.Success;
        }

        public override void OnReset()
        {
            _started = false;
            _timer = 0f;
        }
    }
}