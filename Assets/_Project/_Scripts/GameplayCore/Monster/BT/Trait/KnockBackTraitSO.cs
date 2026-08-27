using Shield_Shot.GameplayCore.Monster.BT.Core;
using Shield_Shot.GameplayCore.Monster.BT.Nodes;
using Shield_Shot.GameplayCore.Monster.BT.Traits;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.BT.Trait
{
    [CreateAssetMenu(menuName = "Monster/BT Trait/Knockback")]
    public class KnockbackTraitSO : BtTraitSO
    {
        [SerializeField] private float _knockbackDistance = 1f;
        [SerializeField] private float _knockbackDuration = 0.1f;

        public override BtNode<BtContext> CreateNode()
            => new KnockbackNode(_knockbackDistance, _knockbackDuration);
    }
}