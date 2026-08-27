using UnityEngine;
using Shield_Shot.GameplayCore.Weapon.Projectile;

namespace Shield_Shot.GameplayCore.Monster.Core
{
    public class DeadLineDetector : MonoBehaviour
    {
        private MonsterBase _monsterBase;

        private void Awake()
        {
            _monsterBase = GetComponentInParent<MonsterBase>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("DeadLine")) return;
            if (_monsterBase == null) return;
            if (_monsterBase.Health != null && _monsterBase.Health.IsDead) return;

            // 돌진 컴포넌트가 있으면 돌진 시작, 없으면 기존처럼 사라짐(폴백)
            if (_monsterBase.TryGetComponent<PlayerCharger>(out var charger))
                charger.BeginCharge();
            else
                _monsterBase.OnReturnToPool?.Invoke(_monsterBase);
        }
    }
}
