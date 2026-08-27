using UnityEngine;
using Shield_Shot.GameplayCore.Render;
using Shield_Shot.GameplayCore.Weapon.Core;
using Shield_Shot.InputSystem.Data;
using Shield_Shot.Audio;

namespace Shield_Shot.GameplayCore.Weapon.Gun
{
    public class SniperWeapon : WeaponBase
    {
        public override WeaponType Type => WeaponType.Sniper;

        [Header("Sniper Animations & Visuals")]
        [SerializeField] private Animator _animator;
        [Tooltip("총구에 붙여둘 프리뷰용 저격 레이저/조준 외형 오브젝트")]
        [SerializeField] private GameObject visualLaser;

        [Header("Sound")]
        [SerializeField] private AudioClip _sniperSfx;
        [SerializeField] private float _sniperVolume = 1f;
        protected override Transform EffectSpawnPoint => firePoint;

        private static readonly int FireHash = Animator.StringToHash("Fire");
        private static readonly int ReloadHash = Animator.StringToHash("Reload");

        public override void Initialize()
        {
            base.Initialize();
            if (_animator == null) _animator = GetComponentInChildren<Animator>();
            if (visualLaser != null) visualLaser.SetActive(false);
            ResetChargeVFXStates();

            var statCalc = GetComponentInChildren<Projectile.StatCalculator>();
            if (statCalc != null && statCalc.WeaponData != null)
            {
                attackCooldownDuration = statCalc.WeaponData.FinalFireRate;
                Debug.Log($"[SniperWeapon] SO 데이터 연동 완료. 적용된 쿨타임(FireRate): {attackCooldownDuration}초");
            }
        }

        public override void HandleInputStay(InputContext ctx)
        {
            base.HandleInputStay(ctx);

            if (IsCooldownActive)
            {
                if (visualLaser != null && visualLaser.activeSelf)
                    visualLaser.SetActive(false);
                return;
            }

            if (visualLaser != null && !visualLaser.activeSelf)
                visualLaser.SetActive(true);

            // 차징 상태일 때만 VFX 재생
            if (ctx.state == GestureState.Charging || ctx.state == GestureState.ChargedComplete)
            {
                HandleChargeVFX(ChargeRatio);
            }
            else
            {
                // 차징 전 상태에선 VFX 타이머/플래그 리셋만 유지
                ResetChargeVFXStates();
            }

            if (CameraFXManager.Instance != null)
                CameraFXManager.Instance.SetZoomByCharge(ChargeRatio);
        }

        public override void HandleInputUp(InputContext ctx)
        {
            if (IsCooldownActive)
            {
                Deactivate();
                return;
            }

            Debug.Log("[SniperWeapon] 스나이퍼 격발 프로토콜 가동");

            if (_animator != null) _animator.SetTrigger(FireHash);

            if (CameraFXManager.Instance != null)
                CameraFXManager.Instance.PlayShotKick(1.5f);

            if (visualLaser != null) visualLaser.SetActive(false);
            ResetChargeVFXStates();

            bool isCriticalShot = true;
            float finalFireRatio = 1.0f;

            if (projectileFireHandler != null)
                projectileFireHandler.Fire(firePoint, AimDirection, finalFireRatio, isCriticalShot);
            else if (projectileLauncher != null)
                projectileLauncher.Fire(firePoint, AimDirection, finalFireRatio, isCriticalShot, WeaponType.Sniper);

            SoundManager.Instance.PlaySFX(_sniperSfx, _sniperVolume);

            StartAttackCooldown(attackCooldownDuration);

            if (_animator != null) _animator.SetTrigger(ReloadHash);

            Deactivate();
        }

        public override void Deactivate()
        {
            base.Deactivate();

            if (visualLaser != null)
                visualLaser.SetActive(false);

            if (CameraFXManager.Instance != null)
                CameraFXManager.Instance.ResetZoom();

            // 이전에 풀에서 꺼낸 VFX는 VFXPoolManager가 자동 반환하므로
            ResetChargeVFXStates();
        }
    }
}