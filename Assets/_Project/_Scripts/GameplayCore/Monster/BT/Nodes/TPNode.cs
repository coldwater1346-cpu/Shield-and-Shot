using Shield_Shot.GameplayCore.Monster.BT;
using Shield_Shot.GameplayCore.Monster.BT.Core;
using Shield_Shot.GameplayCore.Render;          // VFXType, VFXPoolManager
using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.BT.Nodes
{
    public class TPNode : BtNode<BtContext>
    {
        private readonly float _interval;
        private readonly float _pauseDuration;
        private readonly float _teleportDistance;
        private readonly VFXType _vfxType;
        private readonly float _vfxReleaseTime;

        private float _timer;
        private bool _pausing;
        private float _pauseTimer;

        public TPNode(float interval, float pauseDuration, float teleportDistance,
                      VFXType vfxType = VFXType.None, float vfxReleaseTime = 1.5f)
        {
            _interval = interval;
            _pauseDuration = pauseDuration;
            _teleportDistance = teleportDistance;
            _vfxType = vfxType;
            _vfxReleaseTime = vfxReleaseTime;
            _timer = interval;
        }

        public override NodeState Execute(BtContext ctx)
        {
            if (_pausing)
            {
                _pauseTimer -= Time.deltaTime;
                if (_pauseTimer > 0f)
                    return NodeState.Running;

                // 멈춤 끝 → 순간이동 (출발/도착 양쪽에 이펙트)
                Vector3 fromPos = ctx.Transform.position;
                Vector3 fwd = ctx.Transform.forward;
                fwd.y = 0f;
                Vector3 toPos = fromPos + fwd.normalized * _teleportDistance;

                PlayVFX(fromPos);                       // 출발 지점
                ctx.Transform.position = toPos;
                PlayVFX(toPos);                         // 도착 지점

                _pausing = false;
                _timer = _interval;
                return NodeState.Failure;
            }

            _timer -= Time.deltaTime;
            if (_timer > 0f)
                return NodeState.Failure;

            ctx.Movement.Stop();
            _pausing = true;
            _pauseTimer = _pauseDuration;
            return NodeState.Running;
        }

        private void PlayVFX(Vector3 pos)
        {
            if (_vfxType == VFXType.None || VFXPoolManager.Instance == null) return;
            VFXPoolManager.Instance.SpawnVFX(_vfxType, pos, Quaternion.identity, _vfxReleaseTime);
        }

        public override void OnReset()
        {
            _timer = _interval;
            _pausing = false;
            _pauseTimer = 0f;
        }
    }
}