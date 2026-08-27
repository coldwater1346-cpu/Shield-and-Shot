using UnityEngine;
using UnityEngine.UI;

namespace Shield_Shot.UI
{
    public class PlayerHealthUI : MonoBehaviour
    {
        [SerializeField] private Slider _healthSlider;
        [SerializeField] private float _maxHealth = 100f;
        //TODO: 플레이어 체력 값 조정

        private void OnEnable()
        {
            UIEventBus.OnPlayerHealthChanged += UpdateHealthBar;
        }
        private void OnDisable()
        {
            UIEventBus.OnPlayerHealthChanged -= UpdateHealthBar;
        }

        private void UpdateHealthBar(float currentHP)
        {
            _healthSlider.value = currentHP / _maxHealth;
        }
    }

}
