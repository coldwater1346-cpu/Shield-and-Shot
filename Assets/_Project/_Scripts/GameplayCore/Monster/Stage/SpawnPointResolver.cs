using Shield_Shot.GameplayCore.Field;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.Stage
{
    /// 스폰 위치 결정.
    /// 위치 1순위 = 셀 그리드(ArenaSpawnPointProvider). provider 없을 때만 origin/range 폴백.
    /// 회전 = 항상 _spawnOrigin (그리드 포즈의 rotation은 쓰지 않음, 원본 동작 유지).
    public class SpawnPointResolver : MonoBehaviour
    {
        [Header("1순위: 셀 그리드")]
        [SerializeField] private ArenaSpawnPointProvider _provider;
        [SerializeField] private bool _useGrid = true;

        [Header("폴백/회전 기준")]
        [SerializeField] private Transform _spawnOrigin;     // 회전 + 폴백 위치
        [SerializeField] private float _spawnRangeX = 5f;    // 폴백 랜덤 폭
        [SerializeField] private Transform _bossSpawnPoint;  // 보스 폴백

        [Header("Y 고정")]
        [SerializeField] private bool _forceMonsterSpawnY = true;
        [SerializeField] private float _monsterSpawnY = 0f;

        // ── 몬스터 ────────────────────────────────
        public Vector3 GetMonsterSpawnCenter()
        {
            if (TryGetGrid(out var g) && g.TryGetRandomMonsterSpawnPose(out Pose pose))
                return ResolveY(pose.position);          // 그리드 셀
            return ResolveY(GetFallbackCenter());        // 폴백
        }

        public Quaternion GetMonsterSpawnRotation()
            => _spawnOrigin != null ? _spawnOrigin.rotation : Quaternion.identity;

        // ── 보스 ──────────────────────────────────
        public Pose GetBossSpawnPose()
        {
            if (TryGetGrid(out var g) && g.TryGetBossSpawnPose(out Pose pose))
            {
                pose.position = ResolveY(pose.position); // 그리드 보스 셀
                return pose;
            }
            if (_bossSpawnPoint != null)
                return new Pose(ResolveY(_bossSpawnPoint.position), _bossSpawnPoint.rotation);
            return new Pose(ResolveY(GetFallbackCenter()), GetMonsterSpawnRotation());
        }

        // ── 공통 ──────────────────────────────────
        public Vector3 ResolveY(Vector3 p)
        {
            if (_forceMonsterSpawnY) p.y = _monsterSpawnY;
            return p;
        }

        private Vector3 GetFallbackCenter()
        {
            float x = Random.Range(-_spawnRangeX, _spawnRangeX);
            Vector3 origin = _spawnOrigin != null ? _spawnOrigin.position : transform.position;
            return origin + new Vector3(x, 0f, 0f);
        }

        private bool TryGetGrid(out ArenaSpawnPointProvider provider)
        {
            if (!_useGrid) { provider = null; return false; }
            if (_provider == null) _provider = FindFirstObjectByType<ArenaSpawnPointProvider>();
            provider = _provider;
            return provider != null;
        }
    }
}