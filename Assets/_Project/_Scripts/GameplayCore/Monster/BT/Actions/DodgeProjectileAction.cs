using Shield_Shot.GameplayCore.Monster.BT.Core;
using System.Collections.Generic;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.BT.Actions
{
    public class DodgeProjectileAction : ActionNode<BtContext>
    {
        private readonly LayerMask _arrowMask;
        private readonly float _detectRadius;   // 화살 탐지 반경
        private readonly float _hitRadius;      // 이 거리 안으로 지나가면 '맞는다'고 판단
        private readonly float _leadTime;       // 최근접까지 남은 시간이 이 값 이하면 회피 시작
        private readonly float _dodgeSpeed;
        private readonly float _dodgeDuration;
        private readonly float _cooldown;

        private bool _isDodging;
        private float _dodgeTimer;
        private float _cooldownTimer;

        private static readonly Collider[] _buf = new Collider[16];
        private readonly Dictionary<int, Vector3> _lastPos = new();
        private readonly List<int> _seen = new();

        public DodgeProjectileAction(LayerMask arrowMask, float detectRadius, float hitRadius,
                                     float leadTime, float dodgeSpeed, float dodgeDuration, float cooldown)
        {
            _arrowMask = arrowMask;
            _detectRadius = detectRadius;
            _hitRadius = hitRadius;
            _leadTime = leadTime;
            _dodgeSpeed = dodgeSpeed;
            _dodgeDuration = dodgeDuration;
            _cooldown = cooldown;
        }

        public override NodeState Execute(BtContext ctx)
        {
            Debug.Log($"[Dodge] Execute 실행됨 {ctx.Name}");
            if (_isDodging)
            {
                _dodgeTimer -= Time.deltaTime;
                if (_dodgeTimer > 0f) return NodeState.Running;
                _isDodging = false;
                _cooldownTimer = _cooldown;
                return NodeState.Success;
            }

            if (_cooldownTimer > 0f)
            {
                _cooldownTimer -= Time.deltaTime;
                return NodeState.Failure;
            }

            if (!TryFindThreat(ctx, out Vector3 dodgeDir)) return NodeState.Failure;

            ctx.Movement.ApplyKnockback(dodgeDir * _dodgeSpeed, _dodgeDuration);
            _isDodging = true;
            _dodgeTimer = _dodgeDuration;
            return NodeState.Running;
        }

        // 날아오는 화살 중 '나에게 맞을 궤도'인 것만 골라, 피할 방향(궤도 수직)을 반환
        private bool TryFindThreat(BtContext ctx, out Vector3 dodgeDir)
        {
            dodgeDir = Vector3.zero;
            Vector3 m = ctx.Transform.position; m.y = 0f;

            int n = Physics.OverlapSphereNonAlloc(m, _detectRadius, _buf, _arrowMask,
                                                  QueryTriggerInteraction.Collide);
            Debug.Log($"[Dodge] mask={_arrowMask.value} 감지수={n}");
            _seen.Clear();
            float bestTime = float.MaxValue;
            bool found = false;

            for (int i = 0; i < n; i++)
            {
                Collider col = _buf[i];
                int id = col.GetInstanceID();
                Vector3 p = col.transform.position; p.y = 0f;
                _seen.Add(id);

                // 진행 방향은 프레임 간 위치 변화로 계산
                // (isKinematic + transform 이동이면 linearVelocity가 0이라 못 씀)
                if (!_lastPos.TryGetValue(id, out Vector3 prev)) { _lastPos[id] = p; continue; }
                Vector3 delta = p - prev;
                _lastPos[id] = p;
                if (delta.sqrMagnitude < 1e-8f) continue;      // 정지 상태

                Vector3 vel = delta / Mathf.Max(Time.deltaTime, 1e-5f);
                vel.y = 0f;
                float speed = vel.magnitude;
                if (speed < 0.01f) continue;
                Vector3 d = vel / speed;

                float t = Vector3.Dot(m - p, d);
                if (t <= 0f) continue;

                float timeToClosest = t / speed;
                if (timeToClosest > _leadTime) continue;

                Vector3 closest = p + d * t;
                Vector3 offset = m - closest;
                if (offset.magnitude > _hitRadius) continue;

                if (timeToClosest < bestTime)
                {
                    bestTime = timeToClosest;
                    Vector3 perp = Vector3.Cross(Vector3.up, d);
                    float side = Vector3.Dot(offset, perp);
                    dodgeDir = (Mathf.Abs(side) < 0.01f || side >= 0f) ? perp : -perp;
                    found = true;
                }
            }

            // 캐시 정리(풀링된 화살 ID 누적 방지)
            if (_lastPos.Count > 32)
            {
                var keys = new List<int>(_lastPos.Keys);
                foreach (int k in keys) if (!_seen.Contains(k)) _lastPos.Remove(k);
            }

            return found;
        }

        public override void OnReset()
        {
            _isDodging = false;
            _lastPos.Clear();
            _dodgeTimer = 0f;
            _cooldownTimer = 0f;
        }
    }
}