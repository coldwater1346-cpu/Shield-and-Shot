using TMPro;
using UnityEngine;
using Shield_Shot.Core;

namespace Shield_Shot.GameplayCore.Render
{
    public class DamagePopup : MonoBehaviour, IPoolable
    {
        [Header("Settings")]
        [SerializeField] private TextMeshPro _textMesh;
        [SerializeField] private float _moveSpeed = 2f;
        [SerializeField] private float _disappearSpeed = 3f;
        [SerializeField] private float _lifetime = 0.7f;

        private GenericObjectPool<DamagePopup> _pool;
        private Color _textColor;
        private float _timer;

        private void Awake()
        {
            if (_textMesh == null) _textMesh = GetComponent<TextMeshPro>();
            _textColor = _textMesh.color;
        }

        public void OnSpawnedFromPool()
        {
            _textColor = _textMesh.color;
            _textColor.a = 1f;
            _textMesh.color = _textColor;
            _timer = _lifetime;
        }

        public void OnReturnedToPool()
        {
        }

        // 팝업 활성화 및 데이터 세팅 (알파/타이머 리셋은 이제 OnSpawnedFromPool에서 자동 처리됨)
        public void Setup(float damage, GenericObjectPool<DamagePopup> pool)
        {
            _pool = pool;
            _textMesh.text = Mathf.RoundToInt(damage).ToString();
        }

        private void Update()
        {
            // 위로 이동
            transform.Translate(Vector3.up * (_moveSpeed * Time.deltaTime));

            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                // 서서히 투명해지기
                _textColor.a -= _disappearSpeed * Time.deltaTime;
                _textMesh.color = _textColor;

                if (_textColor.a <= 0f)
                {
                    // 풀로 반환
                    gameObject.SetActive(false);
                    _pool?.Return(this);
                }
            }
        }
    }
}