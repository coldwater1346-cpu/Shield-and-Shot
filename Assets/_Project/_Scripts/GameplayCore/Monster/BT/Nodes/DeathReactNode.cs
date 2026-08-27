using Shield_Shot.GameplayCore.Monster.BT.Core;
using Shield_Shot.GameplayCore.Render;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.BT.Nodes
{
    public class DeathReactNode : BtNode<BtContext>
    {
        private readonly float _duration;
        private readonly VFXType _vfxType;

        private float _timer;
        private bool _started;

        public DeathReactNode(float duration = 0.4f, VFXType vfxType = VFXType.None)
        {
            _duration = duration;
            _vfxType = vfxType;
        }

        public override NodeState Execute(BtContext ctx)
        {
            if (!_started)
            {
                ctx.Movement.Stop();
                ctx.SetCollider(false);
                if (_vfxType != VFXType.None)
                    VFXPoolManager.Instance.SpawnVFX(
                        _vfxType,
                        ctx.Transform.position,
                        Quaternion.identity,
                        autoReleaseTime: 2f); // 파티클 길이에 맞게 조정

                _timer = _duration;
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