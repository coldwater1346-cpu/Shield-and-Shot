
using System;
using UnityEngine;
using Shield_Shot.GameplayCore.Common;
using Shield_Shot.UI;

namespace Shield_Shot.GameplayCore.Monster.Core
{
    public class HealthComponent : MonoBehaviour, ITakeDamage
    {
        private bool _isboss = false;
        private int _xpReward;

        public event Action OnDeath;
        public event Action<float> OnDamaged;

        public float CurrentHealth { get; private set; }
        public float MaxHealth { get; private set; }
        public bool IsDead => CurrentHealth <= 0f;

        public bool IsBoss { get => _isboss; set => _isboss = value; }

        // 무적시간 
        [SerializeField] private float _invincibleTimer = 0.5f;
        public bool IsInvincible => _invincibleTimer > 0f;

        public void Initialize(float maxHealth)
        {
            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
            _invincibleTimer = 0f;
        }

        private void Update()
        {
            if (_invincibleTimer > 0f) _invincibleTimer -= Time.deltaTime;
        }

        public void StartInvincibility(float duration)
        {
            _invincibleTimer = Mathf.Max(_invincibleTimer, duration);
        }

        public void TakeDamage(float amount)
        {
            // 이미 사망했거나 무적 상태면 데미지 무시
            if (IsInvincible) return;

            if (IsDead)
            {
                Debug.Log($"[Health] {gameObject.name} 이미 사망 — 데미지 무시");
                return;

            }

            CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
            OnDamaged?.Invoke(amount);

            if (IsBoss)
            {
                UIEventBus.OnBossHealthChanged(CurrentHealth);
            }

            if (IsDead)
            {
                OnDeath?.Invoke();

                if (IsBoss)
                {
                    UIEventBus.OnBossHpBarChanged(false);
                }
            }
        }
    }
}