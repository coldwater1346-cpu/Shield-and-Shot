using Shield_Shot.GameplayCore.Monster.BT.Core;
using Shield_Shot.GameplayCore.Monster.BT.Nodes;
using Shield_Shot.GameplayCore.Monster.BT.Traits;
using Shield_Shot.GameplayCore.Monster.Core;
using Shield_Shot.GameplayCore.Render;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.BT.Trait
{
    [CreateAssetMenu(menuName = "Monster/BT Trait/Split")]
    public class SplitTraitSO : BtTraitSO
    {
        [SerializeField] private int _splitCount = 2;
        [SerializeField] private float _splitRadius = 1f;
        [SerializeField] private float _invincibilityDuration = 0.5f;
        [SerializeField, Range(0f, 1f)] private float _healthPercent = 0.5f;   // 부모 최대체력 대비
        [SerializeField] private VFXType _traitVFXType = VFXType.MonsterTrait;

        public override BtNode<BtContext> CreateNode()
            => new Sequence<BtContext>(
                new DisableCombatNode(),
                new SpawnVfxNode(_traitVFXType),
                new StartInvincibleNode(_invincibilityDuration),
                new PlayAnimNode("Split"),
                new SplitNode(_healthPercent, _splitCount, _splitRadius, _invincibilityDuration));
    }
}