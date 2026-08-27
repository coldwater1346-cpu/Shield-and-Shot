using System.Collections.Generic;
using System;
using Shield_Shot.GameplayCore.Common;
using Shield_Shot.GameplayCore.Render;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.Status
{
    public class StatusEffectController : MonoBehaviour
    {
        public event Action<StatusEffectData> EffectAppliedOrRefreshed;
        public event Action<StatusEffectData> EffectRemoved;

        private sealed class RuntimeStatusEffect
        {
            public StatusEffectData Data;
            public float RemainingTime;
            public float TickTimer;

            public RuntimeStatusEffect(StatusEffectData data)
            {
                Data = data;
                RemainingTime = data.Duration;
                TickTimer = data.TickInterval;
            }
        }

        private readonly Dictionary<string, RuntimeStatusEffect> _activeEffects = new();
        private readonly List<string> _removeBuffer = new();

        private ITakeDamage _damageable;

        private void Awake()
        {
            _damageable = GetComponent<ITakeDamage>();

            if (_damageable == null)
            {
                _damageable = GetComponentInParent<ITakeDamage>();
            }

            if (_damageable == null)
            {
                _damageable = GetComponentInChildren<ITakeDamage>();
            }
        }

        private void Update()
        {
            if (_activeEffects.Count == 0)
            {
                return;
            }

            float deltaTime = Time.deltaTime;
            _removeBuffer.Clear();

            foreach (var pair in _activeEffects)
            {
                RuntimeStatusEffect effect = pair.Value;

                effect.RemainingTime -= deltaTime;

                if (effect.Data.IsDamageOverTime)
                {
                    effect.TickTimer -= deltaTime;

                    if (effect.TickTimer <= 0f)
                    {
                        ApplyTickDamage(effect.Data);
                        effect.TickTimer += effect.Data.TickInterval;
                    }
                }

                if (effect.RemainingTime <= 0f)
                {
                    _removeBuffer.Add(pair.Key);
                }
            }

            for (int i = 0; i < _removeBuffer.Count; i++)
            {
                RemoveEffect(_removeBuffer[i]);
            }
        }

        public void ApplyOrRefresh(StatusEffectData newEffect)
        {
            if (newEffect.Type == StatusEffectType.None || newEffect.Duration <= 0f)
            {
                return;
            }

            string statusID = newEffect.StatusID;

            if (!_activeEffects.TryGetValue(statusID, out RuntimeStatusEffect currentEffect))
            {
                _activeEffects.Add(statusID, new RuntimeStatusEffect(newEffect));
                EffectAppliedOrRefreshed?.Invoke(newEffect);
                return;
            }

            RefreshEffect(currentEffect, newEffect);
            EffectAppliedOrRefreshed?.Invoke(currentEffect.Data);
        }

        public void ClearAll()
        {
            if (_activeEffects.Count > 0)
            {
                _removeBuffer.Clear();

                foreach (var pair in _activeEffects)
                {
                    _removeBuffer.Add(pair.Key);
                }

                for (int i = 0; i < _removeBuffer.Count; i++)
                {
                    RemoveEffect(_removeBuffer[i]);
                }

                return;
            }

            _activeEffects.Clear();
        }

        public bool HasEffect(string statusID)
        {
            return !string.IsNullOrWhiteSpace(statusID) && _activeEffects.ContainsKey(statusID);
        }

        public bool HasEffectType(StatusEffectType type)
        {
            foreach (var pair in _activeEffects)
            {
                if (pair.Value.Data.Type == type)
                {
                    return true;
                }
            }

            return false;
        }

        public float GetMovementSpeedMultiplier()
        {
            float multiplier = 1f;

            foreach (var pair in _activeEffects)
            {
                StatusEffectData data = pair.Value.Data;

                if (data.Type == StatusEffectType.Frozen ||
                    data.Type == StatusEffectType.Stun)
                {
                    return 0f;
                }

                if (data.Type == StatusEffectType.Slow)
                {
                    multiplier = Mathf.Min(multiplier, data.SlowMultiplier);
                }
            }

            return multiplier;
        }

        private void RefreshEffect(RuntimeStatusEffect currentEffect, StatusEffectData newEffect)
        {
            StatusEffectData currentData = currentEffect.Data;

            bool newEffectIsStronger = newEffect.DamagePerSecond > currentData.DamagePerSecond;

            if (newEffectIsStronger)
            {
                currentEffect.Data = newEffect;
                currentEffect.RemainingTime = newEffect.Duration;
                currentEffect.TickTimer = newEffect.TickInterval;
                return;
            }

            currentEffect.RemainingTime = Mathf.Max(currentEffect.RemainingTime, newEffect.Duration);

            if (currentEffect.TickTimer <= 0f && currentEffect.Data.TickInterval > 0f)
            {
                currentEffect.TickTimer = currentEffect.Data.TickInterval;
            }
        }

        private void ApplyTickDamage(StatusEffectData effect)
        {
            if (_damageable == null)
            {
                return;
            }

            _damageable.TakeDamage(effect.DamagePerTick);

            Vector3 effectPosition = transform.position;

            if (effect.ShowDamagePopup && DamagePopupManager.Instance != null)
            {
                DamagePopupManager.Instance.Show(effectPosition, effect.DamagePerTick, false);
            }

            if (effect.TickVfxType != VFXType.None && VFXPoolManager.Instance != null)
            {
                VFXPoolManager.Instance.SpawnVFX(
                    effect.TickVfxType,
                    effectPosition,
                    Quaternion.identity,
                    effect.VfxAutoReleaseTime
                );
            }
        }

        private void RemoveEffect(string statusID)
        {
            if (string.IsNullOrWhiteSpace(statusID) ||
                !_activeEffects.TryGetValue(statusID, out RuntimeStatusEffect effect))
            {
                return;
            }

            _activeEffects.Remove(statusID);
            EffectRemoved?.Invoke(effect.Data);
        }
    }
}
