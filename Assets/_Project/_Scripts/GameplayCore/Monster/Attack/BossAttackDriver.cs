using UnityEngine;
using Shield_Shot.GameplayCore.Monster.Attack;

namespace Shield_Shot.GameplayCore.Monster
{
    [RequireComponent(typeof(AttackComponent))]
    public class BossAttackDriver : MonoBehaviour
    {
        [SerializeField] private float _fireInterval = 2f;

        private AttackComponent _attackComponent;
        private float _timer;

        private void Awake() => _attackComponent = GetComponent<AttackComponent>();
        private void OnEnable() => _timer = _fireInterval;

        private void Update()
        {
            if (!_attackComponent.IsReady) return; // 발사 중이면 타이머 정지

            _timer -= Time.deltaTime;
            if (_timer > 0f) return;

            _timer = _fireInterval;
            _attackComponent.TryExecuteRandom();
        }
    }
}