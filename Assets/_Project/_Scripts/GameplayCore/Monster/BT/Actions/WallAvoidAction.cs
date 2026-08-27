using Shield_Shot.GameplayCore.Monster.BT.Core;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.BT.Actions
{
    public class WallAvoidAction : ActionNode<BtContext>
    {
        private readonly float _probeDistance;   // 전방 감지 거리
        private readonly float _castRadius;      // 몬스터 반지름(통과 가능폭 판정)
        private readonly float _steerSpeed;      // 좌우 회피 속도
        private readonly float _steerHold;       // 스티어 유지 시간(Update→FixedUpdate 보간용)
        private readonly LayerMask _wallMask;
        private readonly float _originHeight;    // 캐스트 원점 높이(지면 자기충돌 방지)

        private const int ScanInterval = 3;      // 몇 프레임에 1회 스캔할지

        // 부채꼴 위스커 각도(도). 0=정면. 몸 반지름으로 쏘므로 '통과 가능'한 방향만 멀리 뚫림
        private static readonly float[] Angles = { 0f, 20f, -20f, 40f, -40f, 60f, -60f };

        // 주기 분산 + 직전 결정 캐싱
        private int _phase = -1;
        private bool _hasSteer;
        private Vector3 _cachedSteer;

        public WallAvoidAction(float probeDistance, float castRadius, float steerSpeed,
                               float steerHold, LayerMask wallMask, float originHeight)
        {
            _probeDistance = probeDistance;
            _castRadius = castRadius;
            _steerSpeed = steerSpeed;
            _steerHold = steerHold;
            _wallMask = wallMask;
            _originHeight = originHeight;
        }

        public override NodeState Execute(BtContext ctx)
        {
            Transform t = ctx.Transform;

            // 인스턴스별 위상으로 스캔 프레임을 분산(특정 프레임에 몰리지 않게)
            if (_phase < 0) _phase = Mathf.Abs(t.GetInstanceID()) % ScanInterval;

            // 스캔 안 하는 프레임: 직전 결정만 유지하고 통과
            if ((Time.frameCount + _phase) % ScanInterval != 0)
            {
                if (_hasSteer) ctx.Movement.SetAvoidSteer(_cachedSteer, _steerHold);
                return NodeState.Failure;
            }

            Vector3 origin = t.position + Vector3.up * _originHeight;
            Vector3 fwd = t.forward; fwd.y = 0f; fwd.Normalize();

            // 정면이 열려 있으면 회피 불필요 → 스티어 해제
            if (!Physics.SphereCast(origin, _castRadius, fwd, out _, _probeDistance,
                                    _wallMask, QueryTriggerInteraction.Ignore))
            {
                _hasSteer = false;
                return NodeState.Failure;
            }

            // 정면이 막혀 여기 왔으므로 반드시 한쪽으로 튼다. 0°(정면)는 후보 제외.
            int preferSign = (Mathf.Abs(t.GetInstanceID()) % 2 == 0) ? 1 : -1;   // 개체별 고정 좌/우
            float bestScore = float.NegativeInfinity;
            Vector3 bestDir = Quaternion.AngleAxis(20f * preferSign, Vector3.up) * fwd;  // 기본값도 측면

            for (int i = 0; i < Angles.Length; i++)
            {
                float ang = Angles[i];
                if (Mathf.Approximately(ang, 0f)) continue;   // ← 정면 제외 (이미 막힘)

                Vector3 dir = Quaternion.AngleAxis(ang, Vector3.up) * fwd;
                float clear = Physics.SphereCast(origin, _castRadius, dir, out RaycastHit hit,
                                                 _probeDistance, _wallMask, QueryTriggerInteraction.Ignore)
                    ? hit.distance
                    : _probeDistance;

                // 더 뚫릴수록 + / 선호측이면 소폭 가산(대칭 벽 결정) / 적게 꺾을수록 소폭 가산
                float sideBias = (Mathf.Sign(ang) == preferSign) ? 0.05f : 0f;
                float score = clear + sideBias - Mathf.Abs(ang) * 0.005f;
                if (score > bestScore) { bestScore = score; bestDir = dir; }
            }

            // bestDir이 항상 측면(≥20°)이라 lateral이 0이 아님 → 정면 벽도 반드시 회피
            Vector3 lateral = Vector3.ProjectOnPlane(bestDir, fwd);   // fwd에 수직인 좌/우 성분
            _cachedSteer = lateral.normalized * _steerSpeed;
            _hasSteer = true;
            ctx.Movement.SetAvoidSteer(_cachedSteer, _steerHold);

            return NodeState.Failure; // 패시브 모디파이어: 나머지 트리(공격 등)도 계속 평가
        }

        public override void OnReset()
        {
            _hasSteer = false;
            _cachedSteer = Vector3.zero;
            // _phase는 인스턴스 고정값이라 유지해도 무방
        }
    }
}