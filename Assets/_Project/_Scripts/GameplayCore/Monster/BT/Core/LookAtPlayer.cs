using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.Core
{
    public class LookAtPlayer : MonoBehaviour
    {
        [SerializeField] private string _playerTag = "Player";
        [Tooltip("회전시킬 대상. 비우면 이 오브젝트(루트).\n일반 몬스터는 모델 자식을 넣으면 이동에 영향 없이 보기만 회전")]
        [SerializeField] private Transform _rotateTarget;
        [Tooltip("초당 회전 각도. 0이면 즉시")]
        [SerializeField] private float _turnSpeed = 360f;
        [Tooltip("Y축만 회전 (위아래 안 기울게)")]
        [SerializeField] private bool _yAxisOnly = true;
        [SerializeField] private float _detectRadius = 50f;

        [Tooltip("시작부터 바라볼지 (보스=켜기 / 일반 몬스터=끄고 돌진 시작 때 활성)")]
        [SerializeField] private bool _activeFromStart = false;

        private static Transform _cachedPlayer; // FindGameObjectWithTag 캐싱용. 파괴되면 null로 자동 재탐색
        private Transform _player;

        private bool _active;

        private void Awake()
        {
            if (_rotateTarget == null) _rotateTarget = transform;
        }

        private void OnEnable()
        {
            _player = null;                 // 풀 재사용 시 다시 탐색
            _active = _activeFromStart;     // 초기 활성 상태 복구
        }

        public void Activate() => _active = true;

        private void Update()
        {
            if (!_active) return;

            if (_player == null)
            {
                _player = FindPlayerByPhysics();
                if (_player == null) return;
            }

            if (_yAxisOnly)
            {
                Vector3 target = _player.position;
                target.y = _rotateTarget.position.y;   // 수평 고정
                RotateTowards(target);
            }
            else
            {
                RotateTowards(_player.position);
            }
        }

        private void RotateTowards(Vector3 targetPos)
        {
            Vector3 dir = targetPos - _rotateTarget.position;
            if (dir.sqrMagnitude < 0.0001f) return;

            Quaternion look = Quaternion.LookRotation(dir);
            _rotateTarget.rotation = _turnSpeed <= 0f
                ? look
                : Quaternion.RotateTowards(_rotateTarget.rotation, look, _turnSpeed * Time.deltaTime);
        }

        //private Transform FindPlayerByPhysics()
        //{
        //    var hits = Physics.OverlapSphere(transform.position, _detectRadius);
        //    Transform nearest = null;
        //    float best = float.MaxValue;
        //    foreach (var h in hits)
        //    {
        //        if (!h.CompareTag(_playerTag)) continue;
        //        float d = (h.transform.position - transform.position).sqrMagnitude;
        //        if (d < best) { best = d; nearest = h.transform; }
        //    }
        //    return nearest;
        //}

        private Transform FindPlayerByPhysics()   // 이름은 유지, 내부만 교체
        {
            // 최초 1회만 태그로 탐색, 파괴되면 자동 재탐색
            if (_cachedPlayer == null)
            {
                var go = GameObject.FindGameObjectWithTag(_playerTag);
                _cachedPlayer = go != null ? go.transform : null;
            }
            if (_cachedPlayer == null) return null;

            // 거리 게이팅(원래 detectRadius)은 물리 없이 벡터 거리로
            float sqr = (_cachedPlayer.position - transform.position).sqrMagnitude;
            return sqr <= _detectRadius * _detectRadius ? _cachedPlayer : null;
        }
    }
}