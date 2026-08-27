using System.Collections;
using Shield_Shot.Audio;
using Shield_Shot.GameplayCore.Render;
using Shield_Shot.GameplayCore.Weapon.Core;
using Shield_Shot.GameplayCore.Weapon.Projectile;
using Shield_Shot.InputSystem.Data;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon
{
    public class ShotgunWeapon : WeaponBase
    {
        public override WeaponType Type => WeaponType.Shotgun;

        [SerializeField] private Transform fireTipPoint;
        protected override Transform EffectSpawnPoint => fireTipPoint;

        [Header("Shotgun Fire Rate Settings")]
        [SerializeField] private float _fireRate = 0.8f;

        [Header("Shotgun Pellet Settings")]
        [SerializeField, Min(1)] private int _pelletCount = 5;
        [SerializeField, Range(0f, 180f)] private float _spreadAngle = 30f;

        [Header("Shotgun Muzzle VFX")]
        [SerializeField] private VFXType _muzzleVfxType = VFXType.Charging;
        [SerializeField] private float _muzzleVfxDuration = 0.5f;

        [Header("Sound")]
        [SerializeField] private AudioClip _shotgunSfx;
        [SerializeField] private AudioClip _reloadSfx;
        [SerializeField] private float _reloadSfxDelay = 0.2f;
        [SerializeField] private float _ShotgunVolume = 1f;

        private float _fireCooldownTimer = 0f;
        private Coroutine _reloadSfxCoroutine;

        private void LateUpdate()
        {
            if (_fireCooldownTimer > 0f)
                _fireCooldownTimer -= Time.deltaTime;
        }

        public override void Initialize()
        {
            base.Initialize();
            var statCalc = GetComponentInChildren<StatCalculator>();
            if (statCalc != null && statCalc.WeaponData != null)
            {
                _fireRate = statCalc.WeaponData.FinalFireRate;
            }
        }

        public override void HandleInputStay(InputContext ctx)
        {
            if (!gameObject.activeInHierarchy) return;

            aimController?.UpdateAimDirection(ctx.dragVector);
            ApplyLocalRotation();
        }

        public override void HandleInputUp(InputContext ctx)
        {
            if (IsCooldownActive)
            {
                Deactivate();
                return;
            }

            // 자체적으로 5발 산탄 폭발 로직을 실행한다.
            ExecuteShotgunBlast(AimDirection, 0f, false);
            Deactivate();
        }

        private void ExecuteShotgunBlast(Vector3 centerDirection, float chargeRatio, bool isCritical)
        {
            if (firePoint == null) return;
            if (projectileFireHandler == null && projectileLauncher == null) return;

            Vector3 baseWorldDir = new Vector3(centerDirection.x, 0f, centerDirection.y).normalized;

            // 5발 산탄 계산 및 투사체 생성기(projectileFireHandler) 호출
            for (int i = 0; i < _pelletCount; i++)
            {
                float angleOffset = CalculatePelletAngle(i);
                Vector3 rotatedWorldDir = Quaternion.AngleAxis(angleOffset, Vector3.up) * baseWorldDir;
                Vector3 pelletDirectionForFire = new Vector3(rotatedWorldDir.x, rotatedWorldDir.z, 0f);

                if (projectileFireHandler != null)
                    projectileFireHandler.Fire(firePoint, pelletDirectionForFire, chargeRatio, isCritical);
                else
                    projectileLauncher.Fire(firePoint, pelletDirectionForFire, chargeRatio, isCritical, WeaponType.Shotgun);
            }

            SoundManager.Instance.PlaySFX(_shotgunSfx, _ShotgunVolume);

            if (_reloadSfxCoroutine != null)
                StopCoroutine(_reloadSfxCoroutine);
            _reloadSfxCoroutine = StartCoroutine(PlayReloadSfxAfterDelay());

            if (_muzzleVfxType != VFXType.None && EffectSpawnPoint != null)
            {
                if (VFXPoolManager.Instance != null)
                    VFXPoolManager.Instance.SpawnVFX(_muzzleVfxType, EffectSpawnPoint.position, EffectSpawnPoint.rotation, _muzzleVfxDuration);
            }

            StartAttackCooldown(_fireRate);
        }

        private float CalculatePelletAngle(int pelletIndex)
        {
            if (_pelletCount <= 1) return 0f;
            float t = (float)pelletIndex / (_pelletCount - 1);
            return Mathf.Lerp(-_spreadAngle * 0.5f, _spreadAngle * 0.5f, t);
        }

        public override void Deactivate()
        {
            base.Deactivate();
        }

        private IEnumerator PlayReloadSfxAfterDelay()
        {
            yield return new WaitForSeconds(_reloadSfxDelay);
            SoundManager.Instance.PlaySFX(_reloadSfx, _ShotgunVolume);
            _reloadSfxCoroutine = null;
        }
    }
}