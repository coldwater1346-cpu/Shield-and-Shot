using Shield_Shot.Audio;
using Shield_Shot.GameplayCore.Render;
using Shield_Shot.GameplayCore.Weapon.Core;
using Shield_Shot.InputSystem.Data;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon
{
    public class BowWeapon : WeaponBase
    {
        public override WeaponType Type => WeaponType.Bow;

        [Header("Animation Settings")]
        [SerializeField] private Animator _animator;

        [Header("Visual Arrow")]
        [Tooltip("firePoint 자식으로 넣어둔 프리뷰용 화살 오브젝트")]
        [SerializeField] private GameObject visualArrow;
        [SerializeField] private Transform arrowTipPoint;

        [Header("Sound")]
        [SerializeField] private AudioClip chargeStartSfx;
        [SerializeField] private AudioClip bowFireSfx;
        [SerializeField] private float chargeSfxTriggerRatio = 0.2f;
        [SerializeField] private float _bowVolume = 1f;

        protected override Transform EffectSpawnPoint => arrowTipPoint;

        private static readonly int ChargeRatioHash = Animator.StringToHash("ChargeRatio");
        private static readonly int FireHash = Animator.StringToHash("Fire");

        private bool _wasCooldownActive;
        private bool _chargeSfxPlayed;

        protected override void Update()
        {
            base.Update();

            // 쿨타임이 진행 중인 동안에는 드래그 입력 여부와 상관없이 화살을 확실히 꺼둔다
            if (IsCooldownActive)
            {
                if (visualArrow != null && visualArrow.activeSelf)
                {
                    visualArrow.SetActive(false);
                }
                _wasCooldownActive = true;
                return;
            }

            // 쿨타임이 방금 막 끝난 순간
            if (_wasCooldownActive && !IsCooldownActive)
            {
                if (visualArrow != null && !visualArrow.activeSelf)
                {
                    // 다음 사격이 가능함을 알리기 위해 프리뷰 화살을 즉시 켜준다
                    visualArrow.SetActive(true);
                }
                _wasCooldownActive = false;
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            if (_animator == null) _animator = GetComponentInChildren<Animator>();

            if (visualArrow != null)
            {
                visualArrow.SetActive(false);
            }

            var statCalc = GetComponentInChildren<Projectile.StatCalculator>();
            if (statCalc != null && statCalc.WeaponData != null)
            {
                attackCooldownDuration = statCalc.WeaponData.FinalFireRate;
                Debug.Log($"[BowWeapon] SO 데이터 연동 완료. 쿨타임: {attackCooldownDuration}초");
            }

            ResetChargeVFXStates();
        }

        public override void HandleInputStay(InputContext ctx)
        {
            base.HandleInputStay(ctx);

            if (IsCooldownActive)
            {
                return;
            }

            RefreshChargePresentation(
                ChargeRatio);
        }
        protected override void OnAttackInputStateApplied(
    float chargeRatio)
        {
            RefreshChargePresentation(
                chargeRatio);
        }
        private void RefreshChargePresentation(
    float chargeRatio)
        {
            if (!_chargeSfxPlayed &&
                chargeRatio >= chargeSfxTriggerRatio)
            {
                _chargeSfxPlayed = true;

                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlaySFX(
                        chargeStartSfx,
                        _bowVolume);
                }
            }

            if (visualArrow != null &&
                !visualArrow.activeSelf)
            {
                visualArrow.SetActive(true);
            }

            HandleChargeVFX(
                chargeRatio);

            if (CameraFXManager.Instance != null)
            {
                CameraFXManager.Instance.SetZoomByCharge(
                    chargeRatio);
            }

            if (_animator != null)
            {
                float animationMapping =
                    chargeRatio *
                    (39f / 45f);

                _animator.SetFloat(
                    ChargeRatioHash,
                    animationMapping);
            }
        }
        public override void HandleInputUp(InputContext ctx)
        {
            if (IsCooldownActive) return;

            _chargeSfxPlayed = false;

            if (_animator != null) _animator.SetTrigger(FireHash);

            if (CameraFXManager.Instance != null)
                CameraFXManager.Instance.PlayShotKick(ChargeRatio);

            if (visualArrow != null) visualArrow.SetActive(false);

            ResetChargeVFXStates();

            bool isCriticalShot = (ChargeRatio >= 0.99f);

            projectileFireHandler?.Fire(firePoint, AimDirection, ChargeRatio, isCriticalShot);
            SoundManager.Instance.PlaySFX(bowFireSfx, _bowVolume);

            StartAttackCooldown();
            Deactivate();
        }

        public override void Deactivate()
        {
            base.Deactivate();
            _chargeSfxPlayed = false;

            if (_animator != null) _animator.SetFloat(ChargeRatioHash, 0f);

            // 장전된 화살 초기화
            if (visualArrow != null && !IsCooldownActive)
            {
                visualArrow.SetActive(false);
            }

            if (CameraFXManager.Instance != null)
            {
                CameraFXManager.Instance.ResetZoom();
            }

            if (arrowTipPoint != null)
            {
                foreach (Transform child in arrowTipPoint)
                {
                    // 혹시라도 자식으로 라이플이나 다른 무기가 들어와 있다면 절대 파괴하지 않는다.
                    if (child.name.Contains("VFX") || child.name.Contains("Clone"))
                    {
                        // 무기 자체가 아니라 연출 오브젝트인 경우에만 안전하게 파괴
                        if (child.GetComponent<WeaponBase>() == null)
                        {
                            Destroy(child.gameObject);
                        }
                    }
                }
            }

            // VFX 초기화
            ResetChargeVFXStates();
        }
    }
}
