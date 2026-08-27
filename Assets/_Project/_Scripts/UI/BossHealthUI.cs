using UnityEngine;
using UnityEngine.UI;

namespace Shield_Shot.UI
{
    public class BossHealthUI : MonoBehaviour
    {
        [SerializeField] private Slider _healthSlider;

        [Tooltip("실제로 보이고 숨겨질 비주얼 루트 (슬라이더가 들어있는 자식 오브젝트). " +
                 "이 컴포넌트가 붙은 GameObject 자체를 SetActive(false)로 끄면, " +
                 "다시 비활성 상태에서는 OnEnable()이 호출되지 않아서 " +
                 "UIEventBus 이벤트 구독 자체가 영원히 재개되지 않는다. " +
                 "그래서 자기 자신이 아니라 이 자식 오브젝트만 켜고 끈다.")]
        [SerializeField] private GameObject _visualRoot;

        [SerializeField] private float _maxHealth = 100f;

        private bool _isOn;

        private void Awake()
        {
            if (_visualRoot == null)
            {
                Debug.LogWarning("[BossHealthUI] VisualRoot가 연결되지 않아 자기 자신을 대신 켜고 끕니다. " +
                                  "이 오브젝트가 씬에서 비활성 상태로 시작하면 이벤트를 못 받을 수 있습니다.");
            }
        }

        private void OnEnable()
        {
            Debug.Log($"[BossHealthUI] OnEnable 호출됨. GameObject 활성 상태: {gameObject.activeInHierarchy}");
            UIEventBus.OnBossHealthChanged += UpdateHealthBar;
            UIEventBus.OnBossHpBarChanged += ToggleBossHpBar;

            // 최초 활성화 시점의 상태를 반영 (평소엔 비주얼 꺼둔 채로 시작)
            SetVisualActive(_isOn);
        }

        private void OnDisable()
        {
            Debug.LogWarning("[BossHealthUI] OnDisable 호출됨 - 이 시점 이후로는 UIEventBus 이벤트를 못 받습니다.");
            UIEventBus.OnBossHealthChanged -= UpdateHealthBar;
            UIEventBus.OnBossHpBarChanged -= ToggleBossHpBar;
        }

        private void UpdateHealthBar(float currentHP)
        {
            if (_healthSlider == null || _maxHealth <= 0f) return;
            _healthSlider.value = currentHP / _maxHealth;
        }

        private void ToggleBossHpBar(bool value)
        {
            Debug.Log($"[BossHealthUI] ToggleBossHpBar 이벤트 수신: {value}, VisualRoot 연결됨: {_visualRoot != null}");
            _isOn = value;
            SetVisualActive(value);

            if (value && _healthSlider != null)
            {
                _healthSlider.value = 1f; // 등장 직후 풀피 상태로 초기화
            }
        }

        private void SetVisualActive(bool active)
        {
            if (_visualRoot != null)
            {
                _visualRoot.SetActive(active);
                Debug.Log($"[BossHealthUI] VisualRoot.SetActive({active}) 실행. 결과: {_visualRoot.activeSelf}");
            }
            else
            {
                // VisualRoot가 없으면 폴백으로 슬라이더만이라도 켜고 끈다.
                if (_healthSlider != null)
                {
                    _healthSlider.gameObject.SetActive(active);
                    Debug.LogWarning($"[BossHealthUI] VisualRoot가 비어있어 Slider 오브젝트를 대신 껐다 켰습니다: {active}");
                }
                else
                {
                    Debug.LogError("[BossHealthUI] VisualRoot도 Slider도 없어서 아무 것도 켜고 끌 수 없습니다!");
                }
            }
        }

        public void SetMaxHealth(float maxHealth)
        {
            if (maxHealth > 0f)
            {
                _maxHealth = maxHealth;
            }
        }
    }
}