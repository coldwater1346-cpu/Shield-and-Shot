using Shield_Shot.GameplayCore.Monster.BT;
using Shield_Shot.GameplayCore.Monster.BT.Core;
using Shield_Shot.GameplayCore.Monster.BT.Nodes;
using Shield_Shot.GameplayCore.Monster.BT.Traits;
using Shield_Shot.GameplayCore.Render;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.BT.Trait
{
    [CreateAssetMenu(menuName = "Monster/BT Trait/TP")]
    public class TPTrait : BtTraitSO
    {
        [SerializeField] private float _interval = 3f;
        [SerializeField] private float _pauseDuration = 0.3f;
        [SerializeField] private float _teleportDistance = 2f;

        [Header("순간이동 이펙트")]
        [SerializeField] private VFXType _vfxType = VFXType.MonsterTrait;
        [SerializeField] private float _vfxReleaseTime = 1.5f;

        public override BtNode<BtContext> CreateNode()
            => new TPNode(_interval, _pauseDuration, _teleportDistance, _vfxType, _vfxReleaseTime);
    }
}