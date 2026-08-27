using Shield_Shot.GameplayCore.Monster.BT.Core;
using Shield_Shot.GameplayCore.Render;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.BT.Nodes
{
    public class SpawnVfxNode : BtNode<BtContext>
    {
        private readonly VFXType _vfxType;
        private readonly float _releaseTime;
        private bool _done;

        public SpawnVfxNode(VFXType vfxType, float releaseTime = 2f)
        {
            _vfxType = vfxType;
            _releaseTime = releaseTime;
        }

        public override NodeState Execute(BtContext ctx)
        {
            if (!_done && _vfxType != VFXType.None && VFXPoolManager.Instance != null)
                VFXPoolManager.Instance.SpawnVFX(_vfxType, ctx.Transform.position,
                                                 Quaternion.identity, _releaseTime);
            _done = true;
            return NodeState.Success;
        }

        public override void OnReset() => _done = false;
    }
}