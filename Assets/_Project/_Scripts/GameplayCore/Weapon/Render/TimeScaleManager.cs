using System.Collections;
using Shield_Shot.GameplayCore.Augment;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Shield_Shot.GameplayCore.Render
{
    public class TimeScaleManager : MonoBehaviour
    {
        public static TimeScaleManager Instance { get; private set; }

        [Header("URP Post-Processing Settings")]
        [Tooltip("씬에 배치된 Global Volume을 여기에 연결하세요")]
        [SerializeField] private Volume globalVolume;

        [Tooltip("투사체 충돌(히트 스톱) 시 테두리에 감돌 붉은 비네트 색상")]
        [SerializeField] private Color hitVignetteColor = Color.red;

        [Tooltip("투사체 충돌 순간 비네트 강도 (0.4 ~ 0.5 추천)")]
        [SerializeField] private float targetVignetteIntensity = 0.45f;

        [Tooltip("타격 순간 화면이 왜곡되는 강도 (오목렌즈 연출 -0.2f)")]
        [SerializeField] private float targetLensDistortion = -0.2f;

        [Header("Camera Hit Pulse Settings")]
        [Tooltip("타격 순간 순간적으로 튕길 FOV 수치 (값이 작을수록 쾅 당겨짐)")]
        [SerializeField] private float hitPulseFOV = 52f;

        [Header("Default Hit Stop Settings (직렬화)")]
        [Tooltip("외부에서 인수를 주지 않았을 때 사용할 기본 타임 스케일 속도 (0.01 ~ 0.1 추천)")]
        [SerializeField] private float defaultHitStopScale = 0.05f;

        private Coroutine _hitStopCoroutine;
        private bool _isHitStopping;

        // URP 컴포넌트 캐싱용 변수
        private Vignette _vignette;
        private LensDistortion _lensDistortion;

        // 복구용 원래 값 저장 변수
        private float _defaultVignetteIntensity;
        private Color _defaultVignetteColor;
        private float _defaultDistortionIntensity;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);

                // 볼륨으로부터 비네트 및 렌즈 왜곡 컴포넌트 추출 및 기본값 저장
                InitPostProcessingCache();
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void InitPostProcessingCache()
        {
            if (globalVolume == null)
            {
                globalVolume = FindFirstObjectByType<Volume>();
            }

            if (globalVolume != null && globalVolume.profile != null)
            {
                // 비네트 캐싱
                if (globalVolume.profile.TryGet(out _vignette))
                {
                    _defaultVignetteIntensity = _vignette.intensity.value;
                    _defaultVignetteColor = _vignette.color.value;
                }
                // 렌즈 왜곡 캐싱
                if (globalVolume.profile.TryGet(out _lensDistortion))
                {
                    _defaultDistortionIntensity = _lensDistortion.intensity.value;
                }
            }
        }

        // 타격 시 순간적으로 시간을 멈추는 히트 스톱 실행 함수
        public void RequestHitStop(bool isCritical, float duration, float? timeScale = null)
        {
            if (!isCritical || AugmentPopupUI.IsOpen)
            {
                return;
            }

            if (_hitStopCoroutine != null)
            {
                StopCoroutine(_hitStopCoroutine);
                ResetVisualsToDefault();
            }

            float finalScale = timeScale ?? defaultHitStopScale;
            _hitStopCoroutine = StartCoroutine(Co_ExecuteHitStop(duration, finalScale));
        }

        private IEnumerator Co_ExecuteHitStop(float duration, float timeScale)
        {
            _isHitStopping = true;
            Time.timeScale = timeScale;

            // 1. 투사체 충돌 순간: 빨간 비네트 + 렌즈 왜곡 + 카메라 줌 펄스 동시 활성화!
            ApplyHitVisualFX(duration);

            // 현실 시간 기준으로 히트 스톱 지속시간만큼 대기
            yield return new WaitForSecondsRealtime(duration);

            // 2. 히트 스톱이 풀리면서 화면 연출 스무스하게 Lerp 복구
            float elapsed = 0f;
            float restoreDuration = 0.15f; // 복구 속도 (초)

            while (elapsed < restoreDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / restoreDuration;

                if (_vignette != null)
                {
                    _vignette.intensity.Override(Mathf.Lerp(targetVignetteIntensity, _defaultVignetteIntensity, t));
                }
                if (_lensDistortion != null)
                {
                    _lensDistortion.intensity.Override(Mathf.Lerp(targetLensDistortion, _defaultDistortionIntensity, t));
                }

                yield return null;
            }

            // 시간 원상 복구 및 최종 값 안전 고정
            // 증강 선택이 진행 중이면 팝업이 시간 정지 상태의 소유자다.
            // 히트스톱이 늦게 끝나더라도 게임 시간을 다시 시작시키지 않는다.
            if (AugmentPopupUI.IsOpen)
            {
                Time.timeScale = 0f;
            }
            else
            {
                Time.timeScale = 1f;
            }
            ResetVisualsToDefault();

            _isHitStopping = false;
            _hitStopCoroutine = null;
        }

        // 투사체 충돌 순간 강렬한 빨간 비네트 및 왜곡 주입
        private void ApplyHitVisualFX(float duration)
        {
            if (_vignette != null)
            {
                _vignette.active = true;
                _vignette.color.Override(hitVignetteColor);
                _vignette.intensity.Override(targetVignetteIntensity);
            }

            if (_lensDistortion != null)
            {
                _lensDistortion.active = true;
                _lensDistortion.intensity.Override(targetLensDistortion);
            }

            // 카메라 줌 펄스 가동
            CameraFXManager.Instance?.TriggerInstantZoomPulse(hitPulseFOV, duration);
        }

        // 화면 연출값 완전 초기화 복구
        private void ResetVisualsToDefault()
        {
            if (_vignette != null)
            {
                _vignette.color.Override(_defaultVignetteColor);
                _vignette.intensity.Override(_defaultVignetteIntensity);
            }
            if (_lensDistortion != null)
            {
                _lensDistortion.intensity.Override(_defaultDistortionIntensity);
            }
        }

        private void OnDestroy()
        {
            Time.timeScale = 1f;
        }
    }
}
