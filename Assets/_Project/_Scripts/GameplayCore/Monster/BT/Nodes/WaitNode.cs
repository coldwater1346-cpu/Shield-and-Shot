using Shield_Shot.GameplayCore.Monster.BT.Core;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.BT.Nodes
{
    public class WaitNode : BtNode<BtContext>
    {
        private readonly float _duration;
        private float _timer;
        private bool _started;

        public WaitNode(float duration) => _duration = duration;

        public override NodeState Execute(BtContext ctx)
        {
            if (!_started) { _timer = _duration; _started = true; }
            _timer -= Time.deltaTime;
            return _timer > 0f ? NodeState.Running : NodeState.Success;
        }

        public override void OnReset() { _started = false; _timer = 0f; }
    }
}