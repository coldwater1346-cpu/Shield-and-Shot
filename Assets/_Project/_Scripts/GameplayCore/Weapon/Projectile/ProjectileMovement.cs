using UnityEngine;
using UnityEngine.EventSystems;

namespace Shield_Shot.GameplayCore.Projectile
{
    [RequireComponent(typeof(Rigidbody))]
    public class ProjectileMovement : MonoBehaviour
    {
        [Header("2.5D 설정")]
        [Tooltip("Y축 고정값. 투사체가 이동할 높이값으로 프로젝트 기준에 맞게 설정.")]
        [SerializeField] private float fixedY = 0f;

        // ============ 내부 상태 ============
        private Rigidbody _rb;
        private Vector3 _moveDirection;
        private float _moveSpeed;
        private bool _isMoving;

        // ============ 프로퍼티 ============
        public Vector3 MoveDirection => _moveDirection; // 현재 진행 방향 벡터
        public float MoveSpeed => _moveSpeed; // 현재 속도값
        public bool IsMoving => _isMoving; // 이동 활성화 여부

        // ============ 유니티 생명주기 ============
        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.useGravity = false;
            _rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        private void FixedUpdate()
        {
            if (!_isMoving) return;
            _rb.linearVelocity = _moveDirection * _moveSpeed;
            EnforceYPosition();
        }

        // 투사체 발사 시 초기화
        public void Launch(Vector3 direction, float speed)
        {
            _moveDirection = new Vector3(direction.x, 0f, direction.z).normalized;
            _moveSpeed = speed;
            _isMoving = true;
            SyncRotationToDirection();
        }

        // 진행 방향 재주입
        public void SetDirection(Vector3 newDirection)
        {
            _moveDirection = new Vector3(newDirection.x, 0f, newDirection.z).normalized;
            SyncRotationToDirection();
        }

        // 이동 즉시 중단
        public void StopMovement()
        {
            _isMoving = false;
            _rb.linearVelocity = Vector3.zero;
        }

        // 오브젝트 풀 반환 전 상태 초기화
        public void ResetState()
        {
            StopMovement();
            _moveDirection = Vector3.forward;
            _moveSpeed = 0f;
            // Y 위치 초기화
            Vector3 pos = transform.position;
            pos.y = fixedY;
            transform.position = pos;
        }

        // Y축 위치를 fixedY로 강제 고정
        private void EnforceYPosition()
        {
            Vector3 pos = _rb.position;
            if (Mathf.Abs(pos.y - fixedY) > 0.001f)
            {
                pos.y = fixedY;
                _rb.MovePosition(pos);
            }
        }

        // 투사체를 진행 방향으로 회전 동기화
        private void SyncRotationToDirection()
        {
            if (_moveDirection == Vector3.zero) return;
            // XZ 평면 기준 Y축 회전
            float angle = Mathf.Atan2(_moveDirection.x, _moveDirection.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }
    }
}