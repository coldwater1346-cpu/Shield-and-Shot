using Shield_Shot.GameplayCore.Monster.BT;
using Shield_Shot.GameplayCore.Monster.BT.Actions;
using Shield_Shot.GameplayCore.Monster.BT.Core;
using Shield_Shot.GameplayCore.Monster.BT.Traits;
using UnityEngine;

[CreateAssetMenu(menuName = "Monster/BT Trait/Dodge")]
public class DodgeTraitSO : BtTraitSO
{
    [Header("탐지")]
    [SerializeField] private LayerMask _arrowMask;          // 플레이어 화살 레이어
    [SerializeField] private float _detectRadius = 15f;
    [SerializeField] private float _hitRadius = 0.8f;       // 몬스터 반지름 + 여유
    [SerializeField] private float _leadTime = 0.35f;       // 피격 예상 0.35초 전에 회피

    [Header("회피")]
    [SerializeField] private float _dodgeSpeed = 5f;
    [SerializeField] private float _dodgeDuration = 0.3f;
    [SerializeField] private float _cooldown = 1.5f;

    public override BtNode<BtContext> CreateNode()
        => new DodgeProjectileAction(_arrowMask, _detectRadius, _hitRadius, _leadTime,
                                     _dodgeSpeed, _dodgeDuration, _cooldown);
}