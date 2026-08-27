using UnityEngine;
using UnityEngine.UI;

namespace Shield_Shot.UI
{
    public class MonsterHPBarUI : MonoBehaviour
    {
        private Slider _slider;
        private Transform _targetMonster;
        private Vector3 _offset;
        private RectTransform _rectTransform;

        private void Awake()
        {
            _slider = GetComponent<Slider>();
            _rectTransform = GetComponent<RectTransform>();
        }

        public void SetupHpBar(Transform monsterTransform, int maxHp, Vector3 offset)
        {
            _targetMonster = monsterTransform;
            _offset = offset;

            _slider.maxValue = maxHp;
            _slider.value = maxHp;

            gameObject.SetActive(true);
            UpdatePosition();
        }

        public void UpdateHp(int currentHp)
        {
            if (_slider != null) _slider.value = currentHp;
        }

        private void LateUpdate()
        {
            if (_targetMonster == null || _targetMonster.gameObject.activeInHierarchy)
            {
                gameObject.SetActive(false);
                return;
            }

            UpdatePosition();
        }

        private void UpdatePosition()
        {
            Vector3 worldPos = _targetMonster.position + _offset;
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

            _rectTransform.position = screenPos;
        }
    }
}

