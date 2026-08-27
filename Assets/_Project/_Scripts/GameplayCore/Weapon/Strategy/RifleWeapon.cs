using Shield_Shot.Audio;
using Shield_Shot.GameplayCore.Render;
using Shield_Shot.GameplayCore.Weapon.Core;
using Shield_Shot.InputSystem.Data;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon
{
    public class RifleWeapon : WeaponBase
    {
        public override WeaponType Type => WeaponType.Rifle;

        [SerializeField] private Transform fireTipPoint;
        protected override Transform EffectSpawnPoint => fireTipPoint;

        [Header("Rifle Aim Line Charge Settings")]
        [Tooltip("조준선이 길어지기 시작하는 문턱 시간 (초)")]
        [SerializeField] private float _chargeThresholdTime = 0.3f;

        [Tooltip("조준선이 최대 길이에 도달하는 데 걸리는 총 홀드 시간 (초)")]
        [SerializeField] private float _maxChargeHoldTime = 1.5f;

        [Header("Rifle Fire Rate Settings")]
        [Tooltip("총알과 총알 사이의 발사 간격 (초)")]
        [SerializeField] private float _fireRate = 0.15f;

        [Header("Rifle Muzzle VFX")]
        [Tooltip("발사 시 총구에서 재생할 VFX 타입 (VFXPoolManager 사용)")]
        [SerializeField] private VFXType _muzzleVfxType = VFXType.Charging;

        [Tooltip("총구 VFX 자동 반환 시간 (초)")]
        [SerializeField] private float _muzzleVfxDuration = 0.5f;

        [Header("Sound")]
        [SerializeField] private AudioClip _rifleFireSfx;
        [SerializeField] private float _rifleVolume = 1f;

        private float _fireCooldownTimer = 0f;

        private void LateUpdate()
        {
            if (_fireCooldownTimer > 0f)
                _fireCooldownTimer -= Time.deltaTime;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _fireCooldownTimer = _fireRate;
        }

        public override void Initialize()
        {
            base.Initialize();

            var statCalc = GetComponentInChildren<Projectile.StatCalculator>();
            if (statCalc != null && statCalc.WeaponData != null)
            {
                _fireRate = statCalc.WeaponData.FinalFireRate;
                Debug.Log($"[RifleWeapon] SO 데이터 연동 완료. 연사 속도: {_fireRate}초");
            }
        }

        public override void HandleInputStay(InputContext ctx)
        {
            if (!gameObject.activeInHierarchy) return;

            aimController?.UpdateAimDirection(ctx.dragVector);
            ApplyLocalRotation();

            if (_fireCooldownTimer <= 0f)
            {
                ExecuteRifleShot();
                HandleChargeVFX(ChargeRatio);
            }

            if (ctx.holdTime >= _chargeThresholdTime)
            {
                float actualChargeTime = ctx.holdTime - _chargeThresholdTime;
                float totalChargeWindow = _maxChargeHoldTime - _chargeThresholdTime;
                chargeController?.SetChargeRatio(Mathf.Clamp01(actualChargeTime / totalChargeWindow));
            }
            else
            {
                chargeController?.Reset();
            }

            if (firePoint != null)
            {
                aimLineRenderer?.Show(this);
                aimLineRenderer?.UpdateLine(AimDirection, ChargeRatio, firePoint.position);
            }
        }

        public override void HandleInputUp(InputContext ctx)
        {
            Debug.Log("[Rifle] 사격 중지 및 조준 해제");
            Deactivate();
        }

        private void ExecuteRifleShot()
        {
            if (firePoint == null || projectileLauncher == null) return;

            projectileLauncher.Fire(firePoint, AimDirection, 0f, false, WeaponType.Rifle);
            SoundManager.Instance.PlaySFX(_rifleFireSfx, _rifleVolume);
            Debug.Log("[Rifle] 사격 중...!");

            // 총구 VFX - VFXPoolManager 사용
            if (_muzzleVfxType != VFXType.None && EffectSpawnPoint != null)
            {
                if (VFXPoolManager.Instance != null)
                    VFXPoolManager.Instance.SpawnVFX(_muzzleVfxType, EffectSpawnPoint.position, EffectSpawnPoint.rotation, _muzzleVfxDuration);
                else
                    Debug.LogWarning("[RifleWeapon] VFXPoolManager 없음. 총구 VFX 스킵.");
            }

            _fireCooldownTimer = _fireRate;
        }

        public override void Deactivate()
        {
            base.Deactivate();
            chargeController?.Reset();
            _fireCooldownTimer = 0f;
            aimLineRenderer?.Hide();
        }
    }
}