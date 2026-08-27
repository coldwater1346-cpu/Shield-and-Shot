// MovementComponent.cs  (Movement/MovementComponent.cs)
using System.Collections.Generic;
using Shield_Shot.GameplayCore.Monster.Status;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.Movement
{
    [RequireComponent(typeof(Rigidbody))]
    public class MovementComponent : MonoBehaviour
    {
        [Header("가속 설정")]
        [SerializeField] private float _acceleration = 8f;
        private Vector3 _currentVelocity;

        private Rigidbody _rb;
        private StatusEffectController _statusEffectController;
        private readonly List<IMovementBehavior> _behaviors = new List<IMovementBehavior>();
        private float _baseSpeed;

        public float BaseSpeed => _baseSpeed;

        private bool _isKnockedBack;
        private float _knockbackTimer;
        private Vector3 _knockbackVelocity;

        // 돌진 설정
        private bool _charging;
        private Transform _chargeTarget;
        private float _chargeSpeed;

        [Header("쏠림 보정")]
        [Header("쏠림 보정")]
        [SerializeField] private float _centeringStrength = 1f;     // 약하게 (기존보다 낮춤)
        [SerializeField] private float _centeringDeadzone = 1.5f;   // 이 안쪽 이탈은 그냥 둠
        [SerializeField] private float _centeringMaxSpeed = 1f;     // 보정 속도 상한(스티어보다 작게)
        [SerializeField] private LayerMask _centeringWallMask;
        [SerializeField] private float _centeringBlockRadius = 0.5f;
        [SerializeField] private float _centeringBlockDistance = 1.2f;

        private float _maneuverTimer;   // 마지막 능동 기동 이후 유예 시간
        public void SuppressCentering(float duration) => _maneuverTimer = Mathf.Max(_maneuverTimer, duration);
        private int _centeringGate;
        private bool _centeringBlocked;

        private Vector3 _spawnPos;

        //벽 회피
        private Vector3 _avoidVelocity;
        private float _avoidTimer;

        //기절 설정
        private float _stunTimer;
        public bool IsStunned => _stunTimer > 0f;
        public void StartStun(float duration) => _stunTimer = duration;


        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _statusEffectController = GetComponent<StatusEffectController>();
        }

        public void Initialize(float speed, List<IMovementBehavior> newBehaviors)
        {
            enabled = true;
            _baseSpeed = speed;
            _behaviors.Clear();
            _behaviors.AddRange(newBehaviors);
            _rb.linearVelocity = Vector3.zero;
            _currentVelocity = Vector3.zero;
            _isKnockedBack = false;
            _charging = false;
            _stunTimer = 0f;
            _spawnPos = transform.position;
        }

        public void ApplyKnockback(Vector3 velocity, float duration)
        {
            _knockbackVelocity = velocity;
            _knockbackTimer = duration;
            _isKnockedBack = true;
            _maneuverTimer = Mathf.Max(_maneuverTimer, duration + 0.5f);   // 회피/넉백 후 그레이스
        }

        private bool VaildKnockBack()
        {
            if (_isKnockedBack)
            {
                _knockbackTimer -= Time.fixedDeltaTime;
                if (_knockbackTimer <= 0f)
                    _isKnockedBack = false;
                else
                {
                    _rb.linearVelocity = _knockbackVelocity;
                    return true;
                }
            }
            return false;
        }

        public void Stop()
        {
            _rb.linearVelocity = Vector3.zero;
            enabled = false;
        }

        // ─── 돌진 ────────────────────────────────────────
        public void StartCharge(Transform target, float speed)
        {
            _charging = true;
            _chargeTarget = target;
            _chargeSpeed = speed;
        }

        public void StopCharge() => _charging = false;

        // 이동 로직
        private void FixedUpdate()
        {
            float speedMul = GetSpeedMultiplier();
            if (speedMul <= 0f) { HardStop(); return; }

            // ── 스턴: enabled와 무관하게 강제 정지 (부활 경직 등) ──
            if (_stunTimer > 0f)
            {
                _stunTimer -= Time.fixedDeltaTime;
                HardStop();
                return;
            }

            if (speedMul <= 0f) { HardStop(); return; }
            if (VaildKnockBack()) return;
            if (TryCharge(speedMul)) return;

            _rb.linearVelocity = CalcNormalVelocity(speedMul);
        }

        // ── 상태이상 속도 배율 ──
        private float GetSpeedMultiplier()
            => _statusEffectController != null
                ? _statusEffectController.GetMovementSpeedMultiplier()
                : 1f;

        // ── 완전 정지(빙결 등) ──
        private void HardStop()
        {
            _currentVelocity = Vector3.zero;
            _rb.linearVelocity = Vector3.zero;
        }

        // ── 돌진 모드: 행동 무시하고 타깃으로 직진 ──
        private bool TryCharge(float speedMul)
        {
            if (!_charging || _chargeTarget == null) return false;

            Vector3 d = _chargeTarget.position - transform.position;
            d.y = 0f;
            _rb.linearVelocity = d.normalized * _chargeSpeed * speedMul;
            return true;
        }

        // ── 일반 이동: 행동 + 벽 회피 + 쏠림 보정 ──
        private Vector3 CalcNormalVelocity(float speedMul)
        {
            Vector3 target = Vector3.zero;
            float modifiedSpeed = _baseSpeed * speedMul;
            for (int i = 0; i < _behaviors.Count; i++)
                target = _behaviors[i].CalculateVelocity(target, modifiedSpeed, transform, Time.time);

            bool avoiding = ApplyAvoidSteer(ref target, speedMul);
            ApplyCentering(ref target, avoiding);

            _currentVelocity = Vector3.MoveTowards(_currentVelocity, target, _acceleration * Time.fixedDeltaTime);
            return _currentVelocity;
        }


        // ── 벽 회피 스티어. 회피 중이면 true ──
        private bool ApplyAvoidSteer(ref Vector3 target, float speedMul)
        {
            if (_avoidTimer <= 0f) return false;

            _avoidTimer -= Time.fixedDeltaTime;
            target += _avoidVelocity * speedMul;
            return true;
        }

        // ── 쏠림 보정: 회피 중엔 억제(좌우 이동 방해하므로) ──
        // ── 쏠림 보정: 회피 중이거나 되돌릴 방향에 벽이 있으면 억제 ──
        //private void ApplyCentering(ref Vector3 target, bool avoiding)
        //{
        //    if (_centeringStrength <= 0f || avoiding) return;

        //    float lateralOffset = Vector3.Dot(transform.position - _spawnPos, transform.right);
        //    if (Mathf.Abs(lateralOffset) < 0.01f) return;

        //    // 스폰 X로 되돌리는 방향
        //    Vector3 pullDir = -transform.right * Mathf.Sign(lateralOffset);

        //    // 되돌리는 방향에 벽이 있으면 보정 스킵(벽으로 밀어넣지 않도록)
        //    Vector3 origin = transform.position + Vector3.up * 0.5f;
        //    if (Physics.SphereCast(origin, _centeringBlockRadius, pullDir, out _,
        //                           _centeringBlockDistance, _centeringWallMask, QueryTriggerInteraction.Ignore))
        //        return;

        //    target += pullDir * Mathf.Abs(lateralOffset) * _centeringStrength;
        //}

        private void ApplyCentering(ref Vector3 target, bool avoiding)
        {
            // 기동 중이거나 방금 기동했으면 보정하지 않음
            if (_maneuverTimer > 0f) { _maneuverTimer -= Time.fixedDeltaTime; return; }
            if (_centeringStrength <= 0f || avoiding) return;

            float lateralOffset = Vector3.Dot(transform.position - _spawnPos, transform.right);
            float absOff = Mathf.Abs(lateralOffset);
            if (absOff < _centeringDeadzone) return;   // 데드존: 작은 이탈은 방치 → 지그재그/회피가 유지됨

            Vector3 pullDir = -transform.right * Mathf.Sign(lateralOffset);

            // 데드존 넘은 만큼만, 상한 걸어 완만하게 (스티어를 못 이기게)
            float excess = absOff - _centeringDeadzone;
            float pullSpeed = Mathf.Min(excess * _centeringStrength, _centeringMaxSpeed);

            // 벽 쪽으론 밀지 않음 (주기 분산)
            if (++_centeringGate >= 3)
            {
                _centeringGate = 0;
                Vector3 origin = transform.position + Vector3.up * 0.5f;
                _centeringBlocked = Physics.SphereCast(origin, _centeringBlockRadius, pullDir, out _,
                    _centeringBlockDistance, _centeringWallMask, QueryTriggerInteraction.Ignore);
            }
            if (_centeringBlocked) return;

            target += pullDir * pullSpeed;
        }

        public void SetAvoidSteer(Vector3 velocity, float duration)
        {
            _avoidVelocity = velocity;
            _avoidTimer = duration;
            _maneuverTimer = Mathf.Max(_maneuverTimer, duration + 0.3f);   // 스티어 후 그레이스
        }
    }
}
