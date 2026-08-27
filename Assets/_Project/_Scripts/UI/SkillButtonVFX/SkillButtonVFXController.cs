using Shield_Shot.GameplayCore.Weapon;
using UnityEngine;
using UnityEngine.UI;

namespace Shield_Shot.GameplayCore.UI
{
    public class SkillButtonVFXController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private WeaponManager _weaponManager;

        [Header("Weapon Cooldown UI")]
        [SerializeField] private Image _weaponCooldownFill;

        [Header("Ready VFX")]
        [SerializeField] private ParticleSystem _weaponReadyVfx;
        [SerializeField] private ParticleSystem _shieldReadyVfx;

        [Header("VFX Raw Images")]
        [SerializeField] private RawImage _weaponVfxRawImage;
        [SerializeField] private RawImage _shieldVfxRawImage;

        [Header("Render Cameras")]
        [SerializeField] private Camera _weaponVfxCamera;
        [SerializeField] private Camera _shieldVfxCamera;

        private bool _wasWeaponReady;
        private bool _wasShieldReady;
        private bool _isStarted;

        private void Start()
        {
            if (_weaponManager == null)
            {
                Debug.LogWarning(
                    "[SkillButtonVFXController] WeaponManager가 연결되지 않았습니다.");

                DisableAllVfx();
                return;
            }

            ApplyCurrentState();
            RefreshWeaponCooldown();

            _isStarted = true;
        }

        private void OnEnable()
        {
            // 최초 활성화에서는 Start가 초기화를 담당한다.
            if (!_isStarted || _weaponManager == null)
                return;

            ApplyCurrentState();
            RefreshWeaponCooldown();
        }

        private void Update()
        {
            if (_weaponManager == null)
                return;

            RefreshWeaponCooldown();
            RefreshWeaponReadyVfx();
            RefreshShieldReadyVfx();
        }

        private void ApplyCurrentState()
        {
            _wasWeaponReady =
                _weaponManager.IsCurrentWeaponSkillReady;

            _wasShieldReady =
                _weaponManager.IsShieldSkillReady;

            SetVfxState(
                _weaponReadyVfx,
                _weaponVfxCamera,
                _weaponVfxRawImage,
                _wasWeaponReady);

            SetVfxState(
                _shieldReadyVfx,
                _shieldVfxCamera,
                _shieldVfxRawImage,
                _wasShieldReady);
        }

        private void RefreshWeaponCooldown()
        {
            if (_weaponCooldownFill == null)
                return;

            float remaining =
                _weaponManager.CurrentWeaponSkillCooldownRemaining;

            float normalized =
                _weaponManager.CurrentWeaponSkillCooldownNormalized;

            bool isCoolingDown = remaining > 0f;

            _weaponCooldownFill.gameObject.SetActive(
                isCoolingDown);

            _weaponCooldownFill.fillAmount =
                isCoolingDown ? normalized : 0f;
        }

        private void RefreshWeaponReadyVfx()
        {
            bool isReady =
                _weaponManager.IsCurrentWeaponSkillReady;

            if (isReady == _wasWeaponReady)
                return;

            SetVfxState(
                _weaponReadyVfx,
                _weaponVfxCamera,
                _weaponVfxRawImage,
                isReady);

            _wasWeaponReady = isReady;
        }

        private void RefreshShieldReadyVfx()
        {
            bool isReady =
                _weaponManager.IsShieldSkillReady;

            if (isReady == _wasShieldReady)
                return;

            SetVfxState(
                _shieldReadyVfx,
                _shieldVfxCamera,
                _shieldVfxRawImage,
                isReady);

            _wasShieldReady = isReady;
        }

        private void SetVfxState(
            ParticleSystem particle,
            Camera renderCamera,
            RawImage rawImage,
            bool active)
        {
            if (active)
            {
                if (rawImage != null)
                    rawImage.enabled = true;

                if (renderCamera != null)
                    renderCamera.enabled = true;

                if (particle != null)
                {
                    particle.gameObject.SetActive(true);
                    particle.Clear(true);
                    particle.Play(true);
                }
            }
            else
            {
                if (particle != null)
                {
                    particle.Stop(
                        true,
                        ParticleSystemStopBehavior.StopEmittingAndClear);

                    particle.Clear(true);
                    particle.gameObject.SetActive(false);
                }

                // RenderTexture에 남은 마지막 프레임을 숨긴다.
                if (rawImage != null)
                    rawImage.enabled = false;

                if (renderCamera != null)
                    renderCamera.enabled = false;
            }
        }

        private void DisableAllVfx()
        {
            SetVfxState(
                _weaponReadyVfx,
                _weaponVfxCamera,
                _weaponVfxRawImage,
                false);

            SetVfxState(
                _shieldReadyVfx,
                _shieldVfxCamera,
                _shieldVfxRawImage,
                false);
        }

        private void OnDisable()
        {
            DisableAllVfx();
        }
    }
}