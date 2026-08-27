using Shield_Shot.GameplayCore.Monster.BT.Actions;
using Shield_Shot.GameplayCore.Monster.BT.Core;
using Shield_Shot.GameplayCore.Monster.BT.Traits;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.BT.Trait
{
    [CreateAssetMenu(menuName = "Monster/BT Trait/Wall Avoid")]
    public class WallAvoidTraitSO : BtTraitSO
    {
        [Header("전방 감지")]
        [SerializeField] private float _probeDistance = 2.5f;  // 벽 감지 거리
        [SerializeField] private float _castRadius = 0.5f;  // 몬스터 반지름(통과폭 = 지름)
        [SerializeField] private float _originHeight = 0.5f;  // 캐스트 원점 높이

        [Header("회피")]
        [SerializeField] private float _steerSpeed = 4f;       // 좌우 회피 속도
        [SerializeField] private float _steerHold = 0.15f;    // 스티어 유지 시간

        [Header("레이어")]
        [SerializeField] private LayerMask _wallMask;          // Wall + 외벽 레이어 지정

        public override BtNode<BtContext> CreateNode()
            => new WallAvoidAction(_probeDistance, _castRadius, _steerSpeed,
                                   _steerHold, _wallMask, _originHeight);
    }
}