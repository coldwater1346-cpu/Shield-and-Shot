using Shield_Shot.GameplayCore.Monster.BT.Core;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.BT.Nodes
{
    // 트리거를 쏘고, 그 클립이 끝날 때까지 Running. 끝나면 Success → 다음 노드 실행.
    public class PlayAnimNode : BtNode<BtContext>
    {
        private readonly int _triggerHash;
        private readonly float _maxWait;   // 전이 실패 시 무한대기 방지 안전장치
        private readonly int _layer;

        private bool _fired;
        private bool _entered;
        private int _fromStateHash;
        private float _timer;

        public PlayAnimNode(string trigger, float maxWait = 3f, int layer = 0)
        {
            _triggerHash = Animator.StringToHash(trigger);
            _maxWait = maxWait;
            _layer = layer;
        }

        public override NodeState Execute(BtContext ctx)
        {
            if (!_fired)
            {
                if (ctx.Anim == null || !ctx.Anim.HasAnimator) return NodeState.Success;
                _fromStateHash = ctx.Anim.CurrentStateHash(_layer);
                ctx.Anim.PlayTrigger(_triggerHash);
                _fired = true; _entered = false; _timer = _maxWait;
            }

            _timer -= Time.deltaTime;
            if (_timer <= 0f) return NodeState.Success;
            if (ctx.Anim.IsInTransition(_layer)) return NodeState.Running;

            if (!_entered)
            {
                if (ctx.Anim.CurrentStateHash(_layer) != _fromStateHash) _entered = true;
                return NodeState.Running;
            }
            return ctx.Anim.CurrentNormalizedTime(_layer) >= 1f ? NodeState.Success : NodeState.Running;
        }

        public override void OnReset()
        {
            _fired = false;
            _entered = false;
            _timer = 0f;
        }
    }
}