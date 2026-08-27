using System.Collections.Generic;
using Shield_Shot.GameplayCore.Monster.Status;
using Shield_Shot.GameplayCore.Render;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Projectile
{
    public class FireGroundArea : MonoBehaviour
    {
        private const string BurnStatusID = "Burn";

        [Header("Burn")]
        [SerializeField] private float _burnDuration = 1f;
        [SerializeField] private float _tickInterval = 0.5f;
        [SerializeField] private float _damagePerTick = 2f;

        [Header("Apply")]
        [SerializeField] private float _applyInterval = 0.35f;

        [Header("Lifetime")]
        [SerializeField] private float _lifeTime = 2f;

        [Header("VFX")]
        [SerializeField] private bool _showDamagePopup = true;
        [SerializeField] private VFXType _tickVfxType = VFXType.Hit;
        [SerializeField] private float _vfxAutoReleaseTime = 1.5f;

        private readonly Dictionary<int, float> _nextApplyTimeByTarget = new();

        private float _expireTime;

        private void OnEnable()
        {
            _expireTime = Time.time + _lifeTime;
            _nextApplyTimeByTarget.Clear();
        }

        private void Update()
        {
            if (Time.time >= _expireTime)
            {
                gameObject.SetActive(false);
            }
        }

        public void Initialize(
            float lifeTime,
            float burnDuration,
            float tickInterval,
            float damagePerTick,
            float applyInterval,
            bool showDamagePopup,
            VFXType tickVfxType,
            float vfxAutoReleaseTime)
        {
            _lifeTime = Mathf.Max(0.1f, lifeTime);
            _burnDuration = Mathf.Max(0.1f, burnDuration);
            _tickInterval = Mathf.Max(0.05f, tickInterval);
            _damagePerTick = Mathf.Max(0f, damagePerTick);
            _applyInterval = Mathf.Max(0.05f, applyInterval);
            _showDamagePopup = showDamagePopup;
            _tickVfxType = tickVfxType;
            _vfxAutoReleaseTime = Mathf.Max(0f, vfxAutoReleaseTime);

            _expireTime = Time.time + _lifeTime;
        }

        private void OnTriggerStay(Collider other)
        {
            StatusEffectController statusController =
                other.GetComponentInParent<StatusEffectController>();

            if (statusController == null)
            {
                return;
            }

            int targetId = statusController.GetInstanceID();
            if (_nextApplyTimeByTarget.TryGetValue(targetId, out float nextApplyTime) &&
                Time.time < nextApplyTime)
            {
                return;
            }

            _nextApplyTimeByTarget[targetId] = Time.time + _applyInterval;

            StatusEffectData burn = new StatusEffectData(
                statusID: BurnStatusID,
                type: StatusEffectType.Burn,
                duration: _burnDuration,
                tickInterval: _tickInterval,
                damagePerTick: _damagePerTick,
                source: this,
                showDamagePopup: _showDamagePopup,
                tickVfxType: _tickVfxType,
                vfxAutoReleaseTime: _vfxAutoReleaseTime
            );

            statusController.ApplyOrRefresh(burn);
        }
    }
}