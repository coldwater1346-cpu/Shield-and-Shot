using Shield_Shot.Core;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Render
{
    public class DamagePopupManager : MonoBehaviour
    {
        public static DamagePopupManager Instance { get; private set; }

        [Header("Prefabs")]
        [SerializeField] private DamagePopup _normalPopupPrefab;
        [SerializeField] private DamagePopup _criticalPopupPrefab;

        [Header("Pool Settings")]
        [SerializeField] private int _initialPoolSize = 20;

        private GenericObjectPool<DamagePopup> _normalPool;
        private GenericObjectPool<DamagePopup> _criticalPool;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (_normalPopupPrefab == null || _criticalPopupPrefab == null)
            {
                Debug.LogError("[DamagePopupManager] 일반 또는 크리티컬 프리팹이 등록되지 않았습니다.");
                return;
            }

            _normalPool = new GenericObjectPool<DamagePopup>(_normalPopupPrefab, _initialPoolSize, transform);
            _criticalPool = new GenericObjectPool<DamagePopup>(_criticalPopupPrefab, _initialPoolSize, transform);
        }

        public void Show(Vector3 position, float damage) => Show(position, damage, false);

        public void Show(Vector3 position, float damage, bool isCritical)
        {
            GenericObjectPool<DamagePopup> targetPool = isCritical ? _criticalPool : _normalPool;
            if (targetPool == null) return;

            float minHeight = isCritical ? 1.8f : 1.5f;
            float maxHeight = isCritical ? 2.4f : 2.0f;

            Vector3 randomOffset = new Vector3(
                Random.Range(-0.3f, 0.3f),
                Random.Range(minHeight, maxHeight),
                Random.Range(-0.3f, 0.3f)
            );

            DamagePopup popup = targetPool.Get();
            popup.transform.position = position + randomOffset;

            if (Camera.main != null)
            {
                Transform cameraTransform = Camera.main.transform;
                popup.transform.rotation = Quaternion.LookRotation(cameraTransform.forward, cameraTransform.up);
            }

            popup.Setup(damage, targetPool);
        }
    }
}