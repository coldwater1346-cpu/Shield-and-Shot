using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.Core
{
    // 이동 벡터(리지드바디 속도) 방향으로 모델을 회전. 시각 전용(이동엔 영향 없음).
    // _rotateTarget은 반드시 '모델 자식'을 넣을 것. 루트를 넣으면 이동 방향이 꼬인다.
    [RequireComponent(typeof(Rigidbody))]
    public class FaceVelocity : MonoBehaviour
    {
        [Tooltip("회전시킬 모델 자식. 비우면 루트(이동 꼬임 주의)")]
        [SerializeField] private Transform _rotateTarget;
        [Tooltip("회전 추종 속도. 클수록 빠르게 따라감(4~12 권장)")]
        [SerializeField] private float _turnSharpness = 8f;
        [Tooltip("이 속도 미만이면 회전 유지(정지 시 지터 방지)")]
        [SerializeField] private float _minSpeed = 0.05f;
        [Tooltip("좌우 성분 과장 배율(시각 전용). 1이면 실제 속도 방향 그대로")]
        [SerializeField] private float _lateralGain = 4f;

        private Vector3 _smoothDir;
        private Rigidbody _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            if (_rotateTarget == null) _rotateTarget = transform;
            _smoothDir = transform.forward;
        }

        private void OnEnable() => _smoothDir = transform.forward;   // 풀 재사용 시 초기화

        private void LateUpdate()
        {
            Vector3 v = _rb.linearVelocity;
            v.y = 0f;
            if (v.sqrMagnitude < _minSpeed * _minSpeed) return;

            Vector3 fwd = transform.forward; fwd.y = 0f; fwd.Normalize();

            float forwardAmount = Vector3.Dot(v, fwd);
            Vector3 lateral = v - fwd * forwardAmount;
            Vector3 lookDir = fwd * forwardAmount + lateral * _lateralGain;
            if (lookDir.sqrMagnitude < 0.0001f) return;

            // 지수 감쇠 스무딩: 프레임레이트 독립적이고 가감속이 자연스러움
            float t = 1f - Mathf.Exp(-_turnSharpness * Time.deltaTime);
            _smoothDir = Vector3.Slerp(_smoothDir, lookDir.normalized, t);

            _rotateTarget.rotation = Quaternion.LookRotation(_smoothDir);
        }
    }
}