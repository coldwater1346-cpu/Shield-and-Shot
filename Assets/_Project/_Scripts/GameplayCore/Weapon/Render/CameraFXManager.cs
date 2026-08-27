using System.Collections;
using UnityEngine;
using Unity.Cinemachine; // 최신 시네머신 v3 대응

namespace Shield_Shot.GameplayCore.Render
{
    [RequireComponent(typeof(CinemachineImpulseSource))]
    public class CameraFXManager : MonoBehaviour
    {
        public static CameraFXManager Instance { get; private set; }

        private CinemachineImpulseSource _impulseSource;

        [Header("Camera References")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private CinemachineCamera virtualCamera;

        [Header("Zoom Settings")]
        [SerializeField] private float defaultFOV = 60f;
        [SerializeField] private float maxZoomInFOV = 45f; // 활 풀차징 시 도달할 FOV
        [SerializeField] private float zoomSpeed = 5f;

        [Header("Shot Kick (Impulse) Settings")]
        [Tooltip("풀차징 발사 시 카메라가 순간적으로 흔들릴 기본 세기")]
        [SerializeField] private float baseKickForce = 0.2f;

        private Coroutine _zoomCoroutine;
        private float _targetFOV;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);

                _impulseSource = GetComponent<CinemachineImpulseSource>();
                if (mainCamera == null) mainCamera = Camera.main;
                if (virtualCamera == null) virtualCamera = FindFirstObjectByType<CinemachineCamera>();

                _targetFOV = defaultFOV;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Update()
        {
            // 매 프레임 타겟 FOV로 부드럽게 보간(Lerp)
            float currentFOV = GetCurrentFOV();
            float nextFOV = Mathf.Lerp(currentFOV, _targetFOV, Time.unscaledDeltaTime * zoomSpeed);
            SetFOV(nextFOV);
        }

        #region 카메라 줌 (FOV) 제어 로직
        // 활 차징 시 실시간으로 조여드는 줌 제어
        public void SetZoomByCharge(float chargeRatio)
        {
            _targetFOV = Mathf.Lerp(defaultFOV, maxZoomInFOV, chargeRatio);
        }

        // 타격(히트 스톱) 순간 카메라를 쿵! 하고 순간 줌인 시키는 함수
        public void TriggerInstantZoomPulse(float pulseFOV, float restoreDelay)
        {
            if (_zoomCoroutine != null) StopCoroutine(_zoomCoroutine);
            _zoomCoroutine = StartCoroutine(Co_ZoomPulse(pulseFOV, restoreDelay));
        }

        private IEnumerator Co_ZoomPulse(float pulseFOV, float restoreDelay)
        {
            SetFOV(pulseFOV);
            _targetFOV = pulseFOV;

            yield return new WaitForSecondsRealtime(restoreDelay);

            _targetFOV = defaultFOV;
        }

        public void ResetZoom()
        {
            if (_zoomCoroutine != null) StopCoroutine(_zoomCoroutine);
            _targetFOV = defaultFOV;
        }
        #endregion

        #region 카메라 흔들림 (Impulse Kick) 제어 로직
        // 화살/총알 발사 순간 카메라에 진동 반동을 주는 함수
        public void PlayShotKick(float chargeRatio)
        {
            if (_impulseSource == null) return;

            // 풀차징(최대 차징)에 가까울 때만 화면을 흔들도록 예외 처리
            if (chargeRatio < 0.95f) return;

            // 시위를 당겼다 놓는 반동 느낌을 주도록 아래로 툭 떨어지는 랜덤 힘 생성
            Vector3 kickDirection = new Vector3(
                Random.Range(-0.5f, 0.5f),
                -1f,
                0f
            ).normalized;

            float finalForce = baseKickForce * chargeRatio;
            _impulseSource.GenerateImpulse(kickDirection * finalForce);
        }
        #endregion

        #region FOV 헬퍼 함수
        private float GetCurrentFOV()
        {
            if (virtualCamera != null) return virtualCamera.Lens.FieldOfView;
            if (mainCamera != null) return mainCamera.fieldOfView;
            return defaultFOV;
        }

        private void SetFOV(float fov)
        {
            if (virtualCamera != null) virtualCamera.Lens.FieldOfView = fov;
            if (mainCamera != null) mainCamera.fieldOfView = fov;
        }
        #endregion
    }
}
