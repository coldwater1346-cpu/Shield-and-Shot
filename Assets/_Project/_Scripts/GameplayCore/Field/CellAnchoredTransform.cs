using UnityEngine;

namespace Shield_Shot.GameplayCore.Field
{
    public class CellAnchoredTransform : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ElementFieldGrid _fieldGrid;

        [Header("Cell")]
        [SerializeField] private Vector2Int _cell = new Vector2Int(8, 3);
        [SerializeField] private bool _clampToGrid = true;
        [SerializeField] private float _yOffset;

        [Header("Apply")]
        [SerializeField] private bool _applyOnAwake = true;
        [SerializeField] private bool _applyOnStart = true;

        [Header("Gizmos")]
        [SerializeField] private bool _drawGizmo = true;
        [SerializeField] private Color _gizmoColor = Color.green;
        [SerializeField, Min(0.01f)] private float _gizmoRadius = 0.25f;

        public Vector2Int Cell
        {
            get => ResolveCell(_cell);
            set => _cell = value;
        }

        public Vector3 WorldPosition => ResolveWorldPosition();

        private void Reset()
        {
            _fieldGrid = FindFirstObjectByType<ElementFieldGrid>();
        }

        private void Awake()
        {
            if (_applyOnAwake)
            {
                Apply();
            }
        }

        private void Start()
        {
            if (_applyOnStart)
            {
                Apply();
            }
        }

        [ContextMenu("Apply Cell Position")]
        public void Apply()
        {
            ElementFieldGrid grid = ResolveFieldGrid();

            if (grid == null)
            {
                Debug.LogWarning("[CellAnchoredTransform] ElementFieldGrid is missing.");
                return;
            }

            transform.position = ResolveWorldPosition(grid);
        }

        private Vector3 ResolveWorldPosition()
        {
            ElementFieldGrid grid = ResolveFieldGrid();
            return grid != null ? ResolveWorldPosition(grid) : transform.position;
        }

        private Vector3 ResolveWorldPosition(ElementFieldGrid grid)
        {
            Vector2Int resolvedCell = ResolveCell(_cell, grid);
            Vector3 position = grid.CellToWorld(resolvedCell);
            return position + grid.FieldSpace.up * _yOffset;
        }

        private Vector2Int ResolveCell(Vector2Int cell)
        {
            ElementFieldGrid grid = ResolveFieldGrid();
            return grid != null ? ResolveCell(cell, grid) : cell;
        }

        private Vector2Int ResolveCell(Vector2Int cell, ElementFieldGrid grid)
        {
            return _clampToGrid ? grid.ClampCell(cell) : cell;
        }

        private ElementFieldGrid ResolveFieldGrid()
        {
            if (_fieldGrid == null)
            {
                _fieldGrid = ElementFieldGrid.Instance != null
                    ? ElementFieldGrid.Instance
                    : FindFirstObjectByType<ElementFieldGrid>();
            }

            return _fieldGrid;
        }

        private void OnDrawGizmosSelected()
        {
            if (!_drawGizmo)
            {
                return;
            }

            ElementFieldGrid grid = ResolveFieldGrid();

            if (grid == null)
            {
                return;
            }

            Gizmos.color = _gizmoColor;
            Gizmos.DrawWireSphere(ResolveWorldPosition(grid), _gizmoRadius);
        }
    }
}
