using UnityEngine;
using Shield_Shot.GameplayCore.Common;          // ITakeDamage
using Shield_Shot.GameplayCore.Monster.Movement;
using Shield_Shot.GameplayCore.Weapon.Projectile;

namespace Shield_Shot.GameplayCore.Monster.Core
{
    public class PlayerCharger : MonoBehaviour
    {
        [SerializeField] private string _playerTag = "Player";
        [SerializeField] private LayerMask _playerMask;   // 플레이어 레이어만
        [SerializeField] private float _detectRadius = 50f;     // 탐지 반경
        [SerializeField] private float _speedMultiplier = 2f;   // 현재 속도 × 배수
        [SerializeField] private float _minChargeSpeed = 4f;    // 너무 느릴 때 하한

        private static Transform _cachedPlayer;
        private Rigidbody _rb;
        private MonsterBase _monster;
        private MovementComponent _movement;
        private bool _charging;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _monster = GetComponent<MonsterBase>();
            _movement = GetComponent<MovementComponent>();
        }

        private void OnDisable() => _charging = false;

        public void BeginCharge()
        {
            if (_charging) return;

            Transform player = FindPlayerByPhysics();
            if (player == null) { _monster.OnReturnToPool?.Invoke(_monster); return; }

            float speed = Mathf.Max(_minChargeSpeed, _rb.linearVelocity.magnitude * _speedMultiplier);

            _charging = true;
            _movement.StartCharge(player, speed);
            GetComponent<LookAtPlayer>()?.Activate();
        }

        private Transform FindPlayerByPhysics()
        {
            if (_cachedPlayer == null)
            {
                var go = GameObject.FindGameObjectWithTag(_playerTag);
                _cachedPlayer = go != null ? go.transform : null;
            }
            if (_cachedPlayer == null) return null;

            float sqr = (_cachedPlayer.position - transform.position).sqrMagnitude;
            return sqr <= _detectRadius * _detectRadius ? _cachedPlayer : null;  // 반경 게이팅 유지
        }

        private void OnTriggerEnter(Collider other) => TryHitPlayer(other);
        private void OnCollisionEnter(Collision col) => TryHitPlayer(col.collider);

        private void TryHitPlayer(Collider other)
        {
            if (!_charging) return;
            if (!other.CompareTag(_playerTag)) return;

            other.GetComponent<PlayerStatus>()?.TakeDamage(_monster.AttackDamage);  // 데이터 기반
            _monster.OnReturnToPool?.Invoke(_monster);
        }
    }
}