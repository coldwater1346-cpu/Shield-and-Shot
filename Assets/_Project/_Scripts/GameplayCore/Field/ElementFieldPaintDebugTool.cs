using UnityEngine;
using UnityEngine.InputSystem;

namespace Shield_Shot.GameplayCore.Field
{
    public class ElementFieldPaintDebugTool : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ElementFieldGrid _fieldGrid;
        [SerializeField] private Transform _paintTarget;

        [Header("Paint")]
        [SerializeField] private ElementType _element = ElementType.Fire;
        [SerializeField, Min(1)] private int _elementLevel = 1;
        [SerializeField] private Vector2Int _cell = new Vector2Int(4, 4);
        [SerializeField] private bool _useTargetPosition = true;
        [SerializeField, Min(0f)] private float _duration = 3f;
        [SerializeField, Min(0f)] private float _radius = 0.8f;

        [Header("Terrain Debug")]
        [SerializeField] private TerrainElementType _snapTerrain = TerrainElementType.Grass;
        [SerializeField] private Vector2Int _lastResolvedCell;
        [SerializeField] private TerrainElementType _lastResolvedTerrain;

        [Header("Input")]
        [SerializeField] private Key _paintKey = Key.P;
        [SerializeField] private Key _clearKey = Key.O;

        private void Reset()
        {
            _fieldGrid = FindFirstObjectByType<ElementFieldGrid>();
            _paintTarget = transform;
        }

        private void Update()
        {
            if (Keyboard.current == null)
            {
                return;
            }

            if (Keyboard.current[_paintKey].wasPressedThisFrame)
            {
                Paint();
            }

            if (Keyboard.current[_clearKey].wasPressedThisFrame)
            {
                PaintNone();
            }
        }

        [ContextMenu("Paint")]
        public void Paint()
        {
            ElementFieldGrid grid = ResolveFieldGrid();

            if (grid == null)
            {
                Debug.LogWarning("[ElementFieldPaintDebugTool] ElementFieldGrid is missing.");
                return;
            }

            Vector3 position = ResolvePaintPosition(grid);
            ElementPaintContext paintContext = new ElementPaintContext(
                _element,
                _elementLevel,
                source: this
            );

            if (_radius > 0f)
            {
                grid.PaintCircle(position, paintContext, _duration, _radius);
            }
            else
            {
                grid.Paint(position, paintContext, _duration);
            }

            Debug.Log(
                $"[ElementFieldPaintDebugTool] Paint {_element} Lv{_elementLevel} at {position}, " +
                $"Duration: {_duration}, Radius: {_radius}"
            );
        }

        [ContextMenu("Clear")]
        public void PaintNone()
        {
            ElementFieldGrid grid = ResolveFieldGrid();

            if (grid == null)
            {
                Debug.LogWarning("[ElementFieldPaintDebugTool] ElementFieldGrid is missing.");
                return;
            }

            Vector3 position = ResolvePaintPosition(grid);

            if (_radius > 0f)
            {
                grid.ClearCircle(position, _radius);
            }
            else
            {
                grid.Clear(position);
            }

            Debug.Log($"[ElementFieldPaintDebugTool] Clear at {position}, Radius: {_radius}");
        }

        [ContextMenu("Log Paint Target Terrain")]
        public void LogPaintTargetTerrain()
        {
            ElementFieldGrid grid = ResolveFieldGrid();

            if (grid == null)
            {
                Debug.LogWarning("[ElementFieldPaintDebugTool] ElementFieldGrid is missing.");
                return;
            }

            Vector3 position = ResolvePaintPosition(grid);
            Vector2Int coord = grid.WorldToCell(position);
            _lastResolvedCell = coord;

            if (!grid.TryGetCellData(coord, out ElementFieldCellData data))
            {
                _lastResolvedTerrain = TerrainElementType.None;
                Debug.Log($"[ElementFieldPaintDebugTool] Paint target is outside grid. World: {position}, Cell: {coord}");
                return;
            }

            _lastResolvedTerrain = data.TerrainElement;
            Debug.Log(
                $"[ElementFieldPaintDebugTool] Paint target terrain. World: {position}, " +
                $"Cell: {coord}, Terrain: {data.TerrainElement}, ActiveElement: {data.CurrentElement}, " +
                $"RemainingTime: {data.RemainingTime:0.00}"
            );
        }

        [ContextMenu("Snap Paint Target To Terrain Cell")]
        public void SnapPaintTargetToTerrainCell()
        {
            ElementFieldGrid grid = ResolveFieldGrid();

            if (grid == null)
            {
                Debug.LogWarning("[ElementFieldPaintDebugTool] ElementFieldGrid is missing.");
                return;
            }

            Vector2Int cellCount = grid.CellCount;

            for (int y = 0; y < cellCount.y; y++)
            {
                for (int x = 0; x < cellCount.x; x++)
                {
                    Vector2Int coord = new Vector2Int(x, y);

                    if (!grid.TryGetCellData(coord, out ElementFieldCellData data) ||
                        data.TerrainElement != _snapTerrain)
                    {
                        continue;
                    }

                    Transform target = _paintTarget != null ? _paintTarget : transform;
                    target.position = grid.CellToWorld(coord);
                    _useTargetPosition = true;
                    _lastResolvedCell = coord;
                    _lastResolvedTerrain = data.TerrainElement;

                    Debug.Log(
                        $"[ElementFieldPaintDebugTool] Snapped paint target to {_snapTerrain}. " +
                        $"Cell: {coord}, World: {target.position}"
                    );
                    return;
                }
            }

            Debug.LogWarning($"[ElementFieldPaintDebugTool] No terrain cell found. Terrain: {_snapTerrain}");
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

        private Vector3 ResolvePaintPosition(ElementFieldGrid grid)
        {
            if (_useTargetPosition && _paintTarget != null)
            {
                return _paintTarget.position;
            }

            return grid.CellToWorld(_cell);
        }
    }
}
