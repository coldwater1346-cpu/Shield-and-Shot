using Shield_Shot.GameplayCore.Monster.BT.Core;
using Shield_Shot.GameplayCore.Monster.BT.Nodes;
using Shield_Shot.GameplayCore.Render;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.BT.Traits
{
    [CreateAssetMenu(menuName = "Monster/BT Trait/Resurrect")]
    public class ResurrectTraitSO : BtTraitSO
    {
        [SerializeField] private int _reviveCount = 1;
        [SerializeField] private float _reviveHealthPercent = 0.5f;
        [SerializeField] private float _stunDuration = 2f;

        [SerializeField] private VFXType _traitVFXType = VFXType.MonsterTrait;
        [SerializeField] private float _reactDuration = 0.4f;

        public override BtNode<BtContext> CreateNode()
       => new Sequence<BtContext>(
           new DisableCombatNode(),
           new SpawnVfxNode(_traitVFXType),
           new StartInvincibleNode(_stunDuration),
           new PlayAnimNode("Resurrect"),
           new ResurrectNode(_reviveCount, _reviveHealthPercent, _stunDuration));
    }
}