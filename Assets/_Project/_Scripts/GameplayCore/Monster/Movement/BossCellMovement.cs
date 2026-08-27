using System.Collections.Generic;
using Shield_Shot.GameplayCore.Field;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.Movement
{
    [CreateAssetMenu(fileName = "BossCellMovement", menuName = "Monster/Movement/BossCellMovement")]
    public class BossCellMovement : ScriptableObject, IMovementBehavior
    {
        [Header("배회 구역 (BossSpawnCell 기준 ± 셀 오프셋)")]
        [SerializeField] private Vector2Int _cellOffsetMin = new Vector2Int(-3, -1);
        [SerializeField] private Vector2Int _cellOffsetMax = new Vector2Int(3, 1);

        [Header("이동")]
        [SerializeField] private float _speedMultiplier = 1f;
        [SerializeField] private float _arriveDistance = 0.3f;
        [SerializeField] private float _waitAtWaypoint = 0.5f; // 웨이포인트 도착 후 대기

        [Header("지형 필터")]
        [SerializeField] private bool _avoidWater = true;

        // SO 에셋은 공유되므로 보스 인스턴스별 상태를 Transform 키로 분리
        private readonly Dictionary<Transform, State> _states = new();
        private ArenaSpawnPointProvider _provider;

        private struct State
        {
            public Vector3 Target;
            public bool HasTarget;
            public float WaitUntil;
        }

        public Vector3 CalculateVelocity(Vector3 currentVelocity, float baseSpeed,
                                         Transform transform, float time)
        {
            if (!TryResolveProvider(out ArenaSpawnPointProvider provider))
            {
                return currentVelocity;
            }

            _states.TryGetValue(transform, out State state);

            // 목표 도착 체크 (수평 거리만)
            if (state.HasTarget)
            {
                Vector3 flat = state.Target - transform.position;
                flat.y = 0f;

                if (flat.sqrMagnitude <= _arriveDistance * _arriveDistance)
                {
                    state.HasTarget = false;
                    state.WaitUntil = time + _waitAtWaypoint;
                }
            }

            // 새 목표 셀 선정
            if (!state.HasTarget && time >= state.WaitUntil &&
                TryPickTarget(provider, out Vector3 target))
            {
                state.Target = target;
                state.HasTarget = true;
            }

            _states[transform] = state;

            if (!state.HasTarget)
            {
                return currentVelocity;
            }

            Vector3 dir = state.Target - transform.position;
            dir.y = 0f;

            if (dir.sqrMagnitude < 0.0001f)
            {
                return currentVelocity;
            }

            return currentVelocity + dir.normalized * (baseSpeed * _speedMultiplier);
        }

        private bool TryPickTarget(ArenaSpawnPointProvider provider, out Vector3 target)
        {
            ElementFieldGrid grid = ElementFieldGrid.Instance;
            Vector2Int center = provider.BossSpawnCell; // 스폰 설정과 자동으로 맞음

            for (int attempt = 0; attempt < 8; attempt++)
            {
                Vector2Int cell = center + new Vector2Int(
                    Random.Range(_cellOffsetMin.x, _cellOffsetMax.x + 1),
                    Random.Range(_cellOffsetMin.y, _cellOffsetMax.y + 1));

                if (grid != null)
                {
                    cell = grid.ClampCell(cell);

                    if (_avoidWater &&
                        grid.TryGetCellData(cell, out ElementFieldCellData data) &&
                        data.TerrainElement == TerrainElementType.Water)
                    {
                        continue; // 물 셀은 목표에서 제외
                    }
                }

                // TryGetCellWorldPose가 지형 높이 스냅까지 처리
                if (provider.TryGetCellWorldPose(cell, out Pose pose))
                {
                    target = pose.position;
                    return true;
                }
            }

            target = default;
            return false;
        }

        private bool TryResolveProvider(out ArenaSpawnPointProvider provider)
        {
            if (_provider == null)
            {
                _provider = FindFirstObjectByType<ArenaSpawnPointProvider>();
            }

            provider = _provider;
            return provider != null;
        }

        private void OnDisable()
        {
            // 씬 전환/도메인 리로드 시 스테일 참조 정리
            _states.Clear();
            _provider = null;
        }
    }
}