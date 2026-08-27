using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Shield_Shot.GameplayCore.Render
{
    public class PostProcessFXController : MonoBehaviour
    {
        // 싱글톤 구조로 BowWeapon에서 언제든 접근 가능하도록 설계
        public static PostProcessFXController Instance { get; private set; }

        [Header("Volume Target")]
        [Tooltip("Vignette 오버라이드가 들어있는 글로벌 볼륨 오브젝트")]
        [SerializeField] private Volume targetVolume;

        [Header("Vignette Settings")]
        [Tooltip("평소(차징 0%)일 때의 비네트 강도")]
        [Range(0f, 1f)][SerializeField] private float minIntensity = 0f;

        [Tooltip("풀차징(100%)일 때 도달할 최대 비네트 강도")]
        [Range(0f, 1f)][SerializeField] private float maxIntensity = 0.45f;

        [Tooltip("차징 진행도에 따라 비네트가 어떤 타이밍에 진해질지 결정할 곡선")]
        [SerializeField] private AnimationCurve intensityCurve = AnimationCurve.Linear(0, 0, 1, 1);

        private Vignette _vignette;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // 볼륨 프로파일에서 Vignette 컴포넌트를 안전하게 가로챔
            if (targetVolume != null && targetVolume.profile != null)
            {
                targetVolume.profile.TryGet(out _vignette);
            }
        }

        private void OnDestroy()
        {
            // 게임이 꺼지거나 오브젝트가 파괴될 때 화면을 원래대로 복구하는 안전장치
            ResetVignette();
        }

        // 활 차징 중 매 프레임 호출하여 비네트 강도를 실시간으로 변조하는 함수
        public void UpdateChargeVignette(float chargeRatio)
        {
            if (_vignette == null) return;

            // 커브 곡선과 연동하여 차징 후반부에 화면이 싹 조여들도록 강도 계산
            float evaluateRatio = intensityCurve.Evaluate(chargeRatio);
            float currentIntensity = Mathf.Lerp(minIntensity, maxIntensity, evaluateRatio);

            // 볼륨 값에 정밀하게 대입
            _vignette.intensity.value = currentIntensity;
        }

        // 💥 발사되거나 조준이 취소되었을 때 화면 효과를 초기화하는 함수
        public void ResetVignette()
        {
            if (_vignette == null) return;
            _vignette.intensity.value = minIntensity;
        }
    }
}
