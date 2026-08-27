using Shield_Shot.GameplayCore.Render;
using Shield_Shot.GameplayCore.Weapon.Projectile;
using System.Collections.Generic;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Shield
{
    public class SlowFieldEffect : MonoBehaviour, IShieldEffect
    {
        [Header("Slow Field Settings")]
        [Tooltip("슬로우 필드 반경 (m)")]
        [SerializeField] private float _radius = 5f;
        [SerializeField, Min(1)] private int _maxProjectilesInField = 32;

        private Collider[] _overlapBuffer;
        private readonly HashSet<ProjectileBase> _inRangeBuffer = new HashSet<ProjectileBase>();
        private readonly List<ProjectileBase> _removeBuffer = new List<ProjectileBase>();

        [Tooltip("필드 내 투사체 속도 배율 (0.3 = 30% 속도)")]
        [SerializeField, Range(0.01f, 0.99f)] private float _slowMultiplier = 0.3f;

        [Tooltip("필드를 벗어난 투사체 속도 복구 속도")]
        [SerializeField] private float _recoverySpeed = 2f;

        [Tooltip("슬로우 적용할 투사체 레이어")]
        [SerializeField] private LayerMask _projectileLayer;

        [Header("Field Center")]
        [Tooltip("슬로우 필드 중심 Transform. 비어있으면 PlayerStatus에서 자동 탐색.")]
        [SerializeField] private Transform _fieldCenter;

        [Header("VFX")]
        [Tooltip("슬로우 필드 활성 중 표시할 VFX 타입")]
        [SerializeField] private VFXType _vfxType = VFXType.None;

        [Tooltip("VFX 프리팹의 기본(원본) 반경. 예: 프리팹이 반경 1m로 디자인되어 있으면 1 입력.")]
        [SerializeField] private float _vfxBaseRadius = 1f;

        private GameObject _activeVfxInstance;
        private bool _isVfxActive;

        private readonly Dictionary<ProjectileBase, float> _slowedProjectiles
            = new Dictionary<ProjectileBase, float>();

        private Vector3 FieldCenter => _fieldCenter != null
            ? _fieldCenter.position
            : transform.position;

        private void Start()
        {
            _overlapBuffer = new Collider[_maxProjectilesInField];
            // 플레이어 중심 Transform 탐색
            // 1순위: 인스펙터 직접 연결
            // 2순위: PlayerStatus
            // 3순위: 이 컴포넌트의 루트 Transform
            if (_fieldCenter != null) return;

            var playerStatus = FindFirstObjectByType<PlayerStatus>();
            if (playerStatus != null)
            {
                _fieldCenter = playerStatus.transform;
                Debug.Log($"[SlowFieldEffect] 플레이어 중심 설정: {_fieldCenter.name}");
            }
            else
            {
                _fieldCenter = transform.root;
                Debug.LogWarning("[SlowFieldEffect] PlayerStatus 없음 → 루트 Transform 사용.");
            }
        }

        private void OnDestroy()
        {
            StopVfx();
        }

        // IShieldEffect: 막을 때 호출
        // 방패에 직접 닿은 투사체는 BlockShield와 동일하게 제거
        public void OnBlock(ProjectileBase projectile, Vector3 hitPosition, Vector3 hitNormal)
        {
            if (projectile == null) return;
            projectile.ReleaseOrDestroy();
            Debug.Log("[SlowFieldEffect] 투사체 차단 및 슬로우 필드 활성 중.");
        }

        private void Update()
        {
            UpdateSlowField();
        }

        private void UpdateSlowField()
        {
            int hitCount = Physics.OverlapSphereNonAlloc(FieldCenter, _radius, _overlapBuffer, _projectileLayer);

            _inRangeBuffer.Clear();

            for (int i = 0; i < hitCount; i++)
            {
                var hit = _overlapBuffer[i];
                var projectile = hit.GetComponent<ProjectileBase>()
                              ?? hit.GetComponentInParent<ProjectileBase>();
                if (projectile == null || !projectile.gameObject.activeSelf) continue;

                _inRangeBuffer.Add(projectile);

                if (!_slowedProjectiles.ContainsKey(projectile))
                    _slowedProjectiles[projectile] = projectile.BaseSpeed;

                Vector3 dir = projectile.Velocity.sqrMagnitude > 0.001f
                    ? projectile.Velocity.normalized
                    : projectile.transform.forward;

                float targetSpeed = _slowedProjectiles[projectile] * _slowMultiplier;
                float currentSpeed = projectile.Velocity.magnitude;
                float newSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, 50f * Time.deltaTime);
                projectile.Velocity = dir * newSpeed;
            }

            _removeBuffer.Clear();
            foreach (var kvp in _slowedProjectiles)
            {
                var projectile = kvp.Key;
                if (projectile == null || !projectile.gameObject.activeSelf) { _removeBuffer.Add(projectile); continue; }
                if (_inRangeBuffer.Contains(projectile)) continue;

                Vector3 dir = projectile.Velocity.sqrMagnitude > 0.001f
                    ? projectile.Velocity.normalized
                    : projectile.transform.forward;

                float newSpeed = Mathf.MoveTowards(
                    projectile.Velocity.magnitude,
                    kvp.Value,
                    _recoverySpeed * kvp.Value * Time.deltaTime);

                projectile.Velocity = dir * newSpeed;
                if (Mathf.Approximately(newSpeed, kvp.Value))
                    _removeBuffer.Add(projectile);
            }
            foreach (var p in _removeBuffer)
                _slowedProjectiles.Remove(p);

            if (_inRangeBuffer.Count > 0)
                EnsureVfx();
            else
                StopVfx();
        }

        private void EnsureVfx()
        {
            if (_isVfxActive) return;
            if (_vfxType == VFXType.None || VFXPoolManager.Instance == null) return;

            // autoReleaseTime 0 → StopVfx에서 직접 반환 제어
            _activeVfxInstance = VFXPoolManager.Instance.SpawnVFX(_vfxType, FieldCenter, Quaternion.identity, 0f);

            if (_activeVfxInstance != null)
            {
                // 슬로우 필드 반경에 맞게 스케일 조정
                float scaleRatio = _radius / Mathf.Max(_vfxBaseRadius, 0.001f);
                _activeVfxInstance.transform.localScale = Vector3.one * scaleRatio;

                // 플레이어를 따라다니도록 부모 설정
                _activeVfxInstance.transform.SetParent(_fieldCenter, true);
                _isVfxActive = true;
            }
        }

        private void StopVfx()
        {
            if (!_isVfxActive) return;

            if (_activeVfxInstance != null && VFXPoolManager.Instance != null)
                VFXPoolManager.Instance.ReturnToPool(_vfxType, _activeVfxInstance);

            _activeVfxInstance = null;
            _isVfxActive = false;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.3f, 0.6f, 1f, 0.2f);
            Gizmos.DrawSphere(FieldCenter, _radius);
            Gizmos.color = new Color(0.3f, 0.6f, 1f, 1f);
            Gizmos.DrawWireSphere(FieldCenter, _radius);
        }
    }
}