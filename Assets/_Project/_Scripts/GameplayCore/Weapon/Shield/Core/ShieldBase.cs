using System.Collections;
using Shield_Shot.GameplayCore.Render;
using Shield_Shot.GameplayCore.Weapon.Projectile;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Shield
{
    [RequireComponent(typeof(ShieldOrbitController))]
    public abstract class ShieldBase : MonoBehaviour
    {
        #region 내부 참조
        protected IShieldColliderDetector colliderDetector;
        protected ShieldOrbitController orbitController;
        #endregion

        public ShieldOrbitController OrbitController => orbitController;

        #region 게이지
        [Header("Shield Gauge")]
        [SerializeField] protected float _maxGauge = 100f;

        protected float _currentGauge = 0f;
        protected bool _isGaugeReady = false;

        public float CurrentGauge => _currentGauge;
        public float MaxGauge => _maxGauge;
        public bool IsGaugeReady => _isGaugeReady;

        public virtual void ChargeGauge(float amount)
        {
            if (_isGaugeReady) return;

            _currentGauge = Mathf.Clamp(_currentGauge + amount, 0f, _maxGauge);

            if (Mathf.Approximately(_currentGauge, _maxGauge))
            {
                _isGaugeReady = true;
                Debug.Log($"[{GetType().Name}] 게이지 MAX! 스킬 사용 가능.");
                StartGlow();
            }
        }

        public virtual void ResetGauge()
        {
            _currentGauge = 0f;
            _isGaugeReady = false;
            StopGlow();
        }
        #endregion

        #region 글로우 효과
        [Header("Gauge Ready Glow")]
        [SerializeField] private Color _glowColor = new Color(0f, 1f, 2f);
        [SerializeField] private float _pulsePeriod = 0.8f;
        [SerializeField] private float _glowMinIntensity = 0.4f;
        [SerializeField] private float _glowMaxIntensity = 2.5f;

        private MeshRenderer _shieldRenderer;
        private Material _shieldMaterialInstance;
        private Color _originalEmissionColor;
        private bool _hadEmission;
        private Coroutine _glowCoroutine;
        private bool _isShaderGraph;

        private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");
        private static readonly int BorderColorID = Shader.PropertyToID("_BorderColor");

        private void InitGlow()
        {
            _shieldRenderer = GetComponentInChildren<MeshRenderer>(true);
            if (_shieldRenderer == null) return;

            _shieldMaterialInstance = _shieldRenderer.material;
            _isShaderGraph = _shieldMaterialInstance.HasColor(BorderColorID);

            if (_isShaderGraph)
                _originalEmissionColor = _shieldMaterialInstance.GetColor(BorderColorID);
            else
            {
                _hadEmission = _shieldMaterialInstance.IsKeywordEnabled("_EMISSION");
                _originalEmissionColor = _shieldMaterialInstance.GetColor(EmissionColorID);
            }
        }

        protected void StartGlow()
        {
            if (_shieldRenderer == null) return;
            StopGlow();
            _glowCoroutine = StartCoroutine(Co_GlowPulse());
        }

        protected void StopGlow()
        {
            if (_glowCoroutine != null)
            {
                StopCoroutine(_glowCoroutine);
                _glowCoroutine = null;
            }
            ResetEmission();
        }

        private IEnumerator Co_GlowPulse()
        {
            if (!_isShaderGraph)
                _shieldMaterialInstance.EnableKeyword("_EMISSION");

            while (true)
            {
                float t = Mathf.PingPong(Time.time / (_pulsePeriod * 0.5f), 1f);
                float intensity = Mathf.Lerp(_glowMinIntensity, _glowMaxIntensity, t);
                Color glowColor = _glowColor * intensity;

                if (_isShaderGraph)
                    _shieldMaterialInstance.SetColor(BorderColorID, glowColor);
                else
                    _shieldMaterialInstance.SetColor(EmissionColorID, glowColor);

                yield return null;
            }
        }

        private void ResetEmission()
        {
            if (_shieldMaterialInstance == null) return;

            if (_isShaderGraph)
            {
                _shieldMaterialInstance.SetColor(BorderColorID, _originalEmissionColor);
            }
            else
            {
                if (_hadEmission)
                    _shieldMaterialInstance.EnableKeyword("_EMISSION");
                else
                    _shieldMaterialInstance.DisableKeyword("_EMISSION");

                _shieldMaterialInstance.SetColor(EmissionColorID, _originalEmissionColor);
            }
        }
        #endregion

        #region Unity 생명주기
        protected virtual void Awake()
        {
            orbitController = GetComponent<ShieldOrbitController>();

            // GetComponent → GetComponentInChildren으로 수정 (자식 오브젝트에 있을 수 있음)
            colliderDetector = GetComponent<IShieldColliderDetector>()
                            ?? GetComponentInChildren<IShieldColliderDetector>();

            if (colliderDetector == null)
            {
                Debug.LogError($"[{GetType().Name}] IShieldColliderDetector 없음. " +
                               "ShieldColliderDetector 또는 NetworkShieldColliderDetector를 추가하세요.");
                return;
            }

            colliderDetector.OnProjectileDetected += HandleProjectileDetected;
            InitGlow();
        }

        protected virtual void OnDestroy()
        {
            if (colliderDetector != null)
                colliderDetector.OnProjectileDetected -= HandleProjectileDetected;

            StopGlow();
            if (_shieldMaterialInstance != null)
                Destroy(_shieldMaterialInstance);
        }
        #endregion

        public void UpdateFromDrag(Vector2 screenDrag)
        {
            orbitController?.UpdateOrbitFromDrag(screenDrag);
        }

        #region 충돌 처리
        private void HandleProjectileDetected(ProjectileBase projectile, Vector3 hitNormal)
        {
            OnProjectileHit_Internal(projectile, hitNormal);
        }

        public void HandleNetworkProjectileHit(ProjectileBase projectile, Vector3 hitNormal)
        {
            OnProjectileHit_Internal(projectile, hitNormal);
        }

        protected abstract void OnProjectileHit_Internal(ProjectileBase projectile, Vector3 hitNormal);
        #endregion
    }
}