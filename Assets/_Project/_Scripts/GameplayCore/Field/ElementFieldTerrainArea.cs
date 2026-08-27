using UnityEngine;

namespace Shield_Shot.GameplayCore.Field
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class ElementFieldTerrainArea : MonoBehaviour
    {
        [SerializeField] private TerrainElementType _terrainElement = TerrainElementType.None;
        [SerializeField] private int _priority;
        [SerializeField] private Collider _areaCollider;
        [SerializeField] private bool _sampleAtColliderCenterY = true;

        public TerrainElementType TerrainElement => _terrainElement;
        public int Priority => _priority;

        private void Reset()
        {
            _areaCollider = GetComponent<Collider>();
        }

        private void OnValidate()
        {
            if (_areaCollider == null)
            {
                _areaCollider = GetComponent<Collider>();
            }
        }

        public bool ContainsWorldPoint(Vector3 worldPoint)
        {
            if (_areaCollider == null || !_areaCollider.enabled)
            {
                return false;
            }

            Bounds bounds = _areaCollider.bounds;
            Vector3 samplePoint = worldPoint;

            if (_sampleAtColliderCenterY)
            {
                samplePoint.y = bounds.center.y;
            }

            if (!bounds.Contains(samplePoint))
            {
                return false;
            }

            Vector3 closestPoint = _areaCollider.ClosestPoint(samplePoint);
            return (closestPoint - samplePoint).sqrMagnitude <= 0.0001f;
        }
    }
}
