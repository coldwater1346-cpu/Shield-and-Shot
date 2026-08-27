using UnityEngine;

namespace Shield_Shot.GameplayCore.Render
{
    [RequireComponent(typeof(CapsuleCollider))]
    public class ColliderOutlineDisplay : MonoBehaviour
    {
        [Header("Outline Settings")]
        [SerializeField] private Color _lineColor = new Color(0f, 1f, 1f, 0.8f);
        [SerializeField] private float _lineWidth = 0.03f;

        [Tooltip("바닥에서 띄울 높이 (Z-Fighting 방지)")]
        [SerializeField] private float _heightOffset = 0.02f;

        [Tooltip("시작 시 자동으로 표시할지 여부")]
        [SerializeField] private bool _showOnStart = true;

        [Header("Circle Settings")]
        [Tooltip("원 분할 수 (높을수록 부드러움)")]
        [SerializeField] private int _circleSegments = 32;

        private CapsuleCollider _capsuleCollider;
        private LineRenderer _lineRenderer;

        private void Awake()
        {
            _capsuleCollider = GetComponent<CapsuleCollider>();
            CreateLineRenderer();

            if (!_showOnStart)
                SetVisible(false);
        }

        private void CreateLineRenderer()
        {
            GameObject lineObj = new GameObject("ColliderOutline");
            lineObj.transform.SetParent(transform, false);

            _lineRenderer = lineObj.AddComponent<LineRenderer>();
            _lineRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"))
            {
                color = _lineColor
            };
            _lineRenderer.material.SetFloat("_Surface", 1); // Transparent
            _lineRenderer.material.renderQueue = 3000;

            _lineRenderer.startColor = _lineColor;
            _lineRenderer.endColor = _lineColor;
            _lineRenderer.startWidth = _lineWidth;
            _lineRenderer.endWidth = _lineWidth;
            _lineRenderer.loop = true;
            _lineRenderer.useWorldSpace = true;
            _lineRenderer.positionCount = _circleSegments;
        }

        private void LateUpdate()
        {
            if (_lineRenderer == null || !_lineRenderer.enabled) return;
            DrawCircleOutline();
        }

        private void DrawCircleOutline()
        {
            // CapsuleCollider의 center를 월드 공간으로 변환
            Vector3 worldCenter = transform.TransformPoint(_capsuleCollider.center);

            // 스케일 적용된 반경 (X/Z 스케일 평균 적용)
            float scaleXZ = (transform.lossyScale.x + transform.lossyScale.z) * 0.5f;
            float radius = _capsuleCollider.radius * scaleXZ;

            // 캡슐 바닥 높이 계산 (direction 0=X,1=Y,2=Z 기준 Y축 캡슐 가정)
            float halfHeight = Mathf.Max(_capsuleCollider.height * 0.5f, radius) * transform.lossyScale.y;
            float y = worldCenter.y - halfHeight + _heightOffset;

            for (int i = 0; i < _circleSegments; i++)
            {
                float angle = i * 2f * Mathf.PI / _circleSegments;
                float px = worldCenter.x + Mathf.Cos(angle) * radius;
                float pz = worldCenter.z + Mathf.Sin(angle) * radius;
                _lineRenderer.SetPosition(i, new Vector3(px, y, pz));
            }
        }

        public void SetVisible(bool isVisible)
        {
            if (_lineRenderer != null)
                _lineRenderer.enabled = isVisible;
        }

        public void ToggleVisible()
        {
            if (_lineRenderer != null)
                _lineRenderer.enabled = !_lineRenderer.enabled;
        }
    }
}