using System.Collections.Generic;
using System;
using Shield_Shot.GameplayCore.Monster.Stage;
using UnityEngine;
using Shield_Shot.UI;

namespace Shield_Shot.GameplayCore.Weapon.Projectile
{
    public class PlayerStatus : MonoBehaviour
    {
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private bool _registerAsLocalPlayer = true;
        [SerializeField] private bool _useStageFallOnDeath = true;
        

        private float _currentHealth = 100f;
        private float _invincibleTimer = 0f;

        [Header("Player Modifier State")]
        public List<ActiveBehavior> CurrentBehaviors = new List<ActiveBehavior>();

        [Header("Next Shot Skill State")]
        [SerializeField] private ProjectileBehaviorSO _nextShotBehavior;
        [SerializeField, Min(1)] private int _nextShotBehaviorLevel = 1;

        public bool IsInvincible => _invincibleTimer > 0f;
        public event Action<PlayerStatus> Died;

        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _maxHealth;
        public float InvincibleTimer { get => _invincibleTimer; set => _invincibleTimer = value; }
        public bool IsDead => _currentHealth <= 0f;
        public bool HasNextShotBehavior => _nextShotBehavior != null;

        private void Awake()
        {
            _currentHealth = _maxHealth;

            if (_registerAsLocalPlayer)
            {
                LocalPlayerStatusContext.Register(this);
            }
        }

        private void Update()
        {
            if (_invincibleTimer > 0f) _invincibleTimer -= Time.deltaTime;
        }

        private void OnDestroy()
        {
            if (_registerAsLocalPlayer)
            {
                LocalPlayerStatusContext.Unregister(this);
            }
        }

        public void AddOrUpgradeBehavior(ProjectileBehaviorSO newSO)
        {
            if (newSO == null) return;

            // 고유 ID를 비교하여 이미 보유 중인 특성인지 인덱스 탐색
            int index = CurrentBehaviors.FindIndex(x =>
                x.BehaviorSO != null && x.BehaviorSO.BehaviorID == newSO.BehaviorID);

            if (index >= 0)
            {
                // 패턴 B: 이미 존재하므로 구조체 꺼내서 레벨만 증싱 후 갱신
                var behavior = CurrentBehaviors[index];
                behavior.Level++;
                CurrentBehaviors[index] = behavior;
                Debug.Log($"{newSO.BehaviorName} 특성이 {behavior.Level} 레벨로 업그레이드되었습니다. (현재 레벨: {behavior.Level})");
            }
            else
            {
                // 처음 뽑힌 특성이면 1레벨로 리스트에 신규 추가
                CurrentBehaviors.Add(new ActiveBehavior(newSO, 1));
                Debug.Log($"새로운 특성 획득: {newSO.BehaviorName}(을)를 1레벨로 설정했습니다.");
            }
        }

        public void EnsureBehaviorRegistered(ProjectileBehaviorSO so)
        {
            if (so == null) return;

            int index = CurrentBehaviors.FindIndex(x => x.BehaviorSO != null && x.BehaviorSO.BehaviorID == so.BehaviorID);
            if (index >= 0) return; // 이미 보유 중이면 레벨을 건드리지 않는다

            CurrentBehaviors.Add(new ActiveBehavior(so, 1));
            Debug.Log($"[PlayerStatus] {so.BehaviorName} 무기 스킬을 1레벨로 등록했습니다.");
        }

        public void EnsureBehaviorRegisteredAtLevel(ProjectileBehaviorSO so, int level)
        {
            if (so == null) return;
            level = Mathf.Max(1, level);

            int index = CurrentBehaviors.FindIndex(x => x.BehaviorSO != null && x.BehaviorSO.BehaviorID == so.BehaviorID);
            if (index >= 0)
            {
                ActiveBehavior behavior = CurrentBehaviors[index];
                if (behavior.Level < level)
                {
                    behavior.Level = level;
                    CurrentBehaviors[index] = behavior;
                    Debug.Log($"[PlayerStatus] {so.BehaviorName} 레벨을 강화 보너스로 {level}까지 끌어올렸습니다.");
                }
                return;
            }

            CurrentBehaviors.Add(new ActiveBehavior(so, level));
            Debug.Log($"[PlayerStatus] {so.BehaviorName}을(를) 강화 보너스 반영 {level}레벨로 등록했습니다.");
        }

        public void RemoveBehavior(ProjectileBehaviorSO so)
        {
            if (so == null) return;

            int index = CurrentBehaviors.FindIndex(x => x.BehaviorSO != null && x.BehaviorSO.BehaviorID == so.BehaviorID);
            if (index >= 0)
            {
                Debug.Log($"[PlayerStatus] {so.BehaviorName} 특성 제거 (무기 스왑).");
                CurrentBehaviors.RemoveAt(index);
            }
        }

        public int GetBehaviorLevel(ProjectileBehaviorSO so)
        {
            if (so == null) return 0;

            int index = CurrentBehaviors.FindIndex(x => x.BehaviorSO != null && x.BehaviorSO.BehaviorID == so.BehaviorID);
            return index >= 0 ? CurrentBehaviors[index].Level : 0;
        }

        public void ResetBaseBehaviors(List<ProjectileBehaviorSO> baseBehaviors)
        {
            CurrentBehaviors.Clear();

            if (baseBehaviors == null) return;

            foreach (var so in baseBehaviors)
            {
                if (so != null)
                    AddOrUpgradeBehavior(so);
            }
        }

        public void ReserveNextShotBehavior(ProjectileBehaviorSO behavior, int level = 1)
        {
            if (behavior == null)
            {
                ClearNextShotBehavior();
                return;
            }

            _nextShotBehavior = behavior;
            _nextShotBehaviorLevel = Mathf.Max(1, level);
        }

        public void ClearNextShotBehavior()
        {
            _nextShotBehavior = null;
            _nextShotBehaviorLevel = 1;
        }

        public bool TryInjectAndConsumeNextShotBehavior(ProjectileBase projectile)
        {
            if (_nextShotBehavior == null)
            {
                return false;
            }

            if (!_nextShotBehavior.CanInject(projectile))
            {
                return false;
            }

            _nextShotBehavior.InjectBehavior(projectile, _nextShotBehaviorLevel);
            ClearNextShotBehavior();
            return true;
        }

        public void StartInvincibility(float duration)
        {
            _invincibleTimer = Mathf.Max(_invincibleTimer, duration);
        }

        public void TakeDamage(float damage)
        {
            if (IsDead || IsInvincible) return;

            _currentHealth -= damage;
            UIEventBus.RaisePlayerHealthChanged(_currentHealth);
            if (_currentHealth <= 0f)
            {
                _currentHealth = 0f;
                Died?.Invoke(this);

                if (_useStageFallOnDeath && StageManager.Instance != null)
                {
                    StageManager.Instance.StageFall();
                }
            }
        }

        public void ResetHP()
        {
            _currentHealth = _maxHealth;
            UIEventBus.RaisePlayerHealthChanged(_currentHealth);
        }
    }
}