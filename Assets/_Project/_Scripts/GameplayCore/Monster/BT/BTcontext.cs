using Shield_Shot.GameplayCore.Monster.Attack;
using Shield_Shot.GameplayCore.Monster.Core;
using Shield_Shot.GameplayCore.Monster.Movement;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.BT
{
    public class BtContext
    {
        // 컴포넌트 캐시 (노드에서 GetComponent 금지)
        public MonsterBase Monster { get; }   // 이스케이프 해치: SourcePrefab 등
        public Transform Transform { get; }
        public HealthComponent Health { get; }
        public MovementComponent Movement { get; }
        public AttackComponent Attack { get; }
        public AnimComponent Anim { get; }

        public BtContext(MonsterBase monster)
        {
            Monster = monster;
            Transform = monster.transform;
            Health = monster.Health;
            Movement = monster.Movement;
            Attack = monster.Attack;
            Anim = monster.GetComponent<AnimComponent>();
        }

        // ── 의도 단위 API ──────────────────────────────
        public bool IsActive => Monster.gameObject.activeInHierarchy;
        public string Name => Monster.name;

        public void SetCollider(bool on) => Monster.SetColliderEnabled(on);

        /// 풀 반환. 콜라이더 해제까지 함께 처리한다.
        public void Despawn()
        {
            Monster.SetColliderEnabled(false);
            Monster.OnReturnToPool?.Invoke(Monster);
        }
    }
}