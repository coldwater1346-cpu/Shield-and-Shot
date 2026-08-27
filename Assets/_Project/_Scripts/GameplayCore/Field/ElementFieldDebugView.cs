using UnityEngine;

namespace Shield_Shot.GameplayCore.Field
{
    public class ElementFieldDebugView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ElementFieldGrid _fieldGrid;

        [Header("Draw")]
        [SerializeField] private bool _drawActiveCells = true;
        [SerializeField] private bool _drawInactiveGrid;
        [SerializeField, Min(0.01f)] private float _activeCellHeight = 0.03f;
        [SerializeField, Range(0.1f, 1f)] private float _activeCellFillRatio = 0.72f;
        [SerializeField, Min(0f)] private float _activeCellYOffset = 0.04f;
        [SerializeField, Range(0.1f, 1f)] private float _inactiveGridFillRatio = 0.98f;

        [Header("Colors")]
        [SerializeField] private Color _inactiveColor = new Color(1f, 1f, 1f, 0.12f);
        [SerializeField] private Color _fireColor = new Color(1f, 0.15f, 0.05f, 0.55f);
        [SerializeField] private Color _iceColor = new Color(0.25f, 0.8f, 1f, 0.55f);
        [SerializeField] private Color _poisonColor = new Color(0.35f, 1f, 0.2f, 0.55f);
        [SerializeField] private Color _lightningColor = new Color(1f, 0.9f, 0.1f, 0.55f);
        [SerializeField] private Color _windColor = new Color(0.75f, 1f, 0.85f, 0.45f);
        [SerializeField] private Color _waterColor = new Color(0.1f, 0.35f, 1f, 0.45f);

        [Header("Terrain Debug")]
        [SerializeField] private bool _drawTerrain = true;
        [SerializeField] private bool _drawNoneTerrain;
        [SerializeField, Range(0f, 1f)] private float _terrainAlpha = 0.18f;
        [SerializeField, Min(0.001f)] private float _terrainCellHeight = 0.01f;
        [SerializeField, Range(0.1f, 1f)] private float _terrainCellFillRatio = 0.96f;
        [SerializeField] private Color _noneTerrainColor = new Color(0.3f, 0.3f, 0.3f, 0.08f);
        [SerializeField] private Color _grassTerrainColor = new Color(0.2f, 0.8f, 0.2f, 0.25f);
        [SerializeField] private Color _sandTerrainColor = new Color(0.9f, 0.75f, 0.35f, 0.22f);
        [SerializeField] private Color _waterTerrainColor = new Color(0.2f, 0.45f, 1f, 0.25f);
        [SerializeField] private Color _iceTerrainColor = new Color(0.65f, 0.9f, 1f, 0.25f);
        [SerializeField] private Color _mudTerrainColor = new Color(0.35f, 0.25f, 0.15f, 0.25f);
        [SerializeField] private Color _rockTerrainColor = new Color(0.45f, 0.45f, 0.45f, 0.25f);

        private void Reset()
        {
            _fieldGrid = FindFirstObjectByType<ElementFieldGrid>();
        }

        private void OnDrawGizmos()
        {
            ElementFieldGrid grid = ResolveFieldGrid();

            if (grid == null)
            {
                return;
            }

            if (_drawTerrain)
            {
                DrawTerrainCells(grid);
            }

            Vector2Int cellCount = grid.CellCount;

            for (int x = 0; x < cellCount.x; x++)
            {
                for (int y = 0; y < cellCount.y; y++)
                {
                    Vector2Int coord = new Vector2Int(x, y);
                    bool hasData = grid.TryGetCellData(coord, out ElementFieldCellData data);

                    if (!hasData || !data.IsActive)
                    {
                        if (_drawInactiveGrid)
                        {
                            DrawInactiveCell(grid, coord);
                        }

                        continue;
                    }

                    if (_drawActiveCells)
                    {
                        DrawActiveCell(grid, coord, data.CurrentElement);
                    }
                }
            }
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

        private void DrawInactiveCell(ElementFieldGrid grid, Vector2Int coord)
        {
            Gizmos.color = _inactiveColor;
            Vector3 center = GetCellCenter(grid, coord, 0f, 0.002f);
            Vector3 size = GetCellSize(grid, _inactiveGridFillRatio, 0.002f);
            Gizmos.DrawWireCube(center, size);
        }

        private void DrawActiveCell(ElementFieldGrid grid, Vector2Int coord, ElementType element)
        {
            Gizmos.color = GetElementColor(element);
            Vector3 center = GetCellCenter(grid, coord, _activeCellYOffset, _activeCellHeight);
            Vector3 size = GetCellSize(grid, _activeCellFillRatio, _activeCellHeight);
            Gizmos.DrawCube(center, size);
        }

        private void DrawTerrainCells(ElementFieldGrid grid)
        {
            Vector2Int cellCount = grid.CellCount;

            for (int x = 0; x < cellCount.x; x++)
            {
                for (int y = 0; y < cellCount.y; y++)
                {
                    Vector2Int coord = new Vector2Int(x, y);

                    if (!grid.TryGetCellData(coord, out ElementFieldCellData data))
                    {
                        continue;
                    }

                    if (!_drawNoneTerrain && data.TerrainElement == TerrainElementType.None)
                    {
                        continue;
                    }

                    Color color = GetTerrainColor(data.TerrainElement);
                    color.a = _terrainAlpha;
                    Gizmos.color = color;

                    Vector3 center = GetCellCenter(grid, coord, 0f, _terrainCellHeight);
                    Vector3 size = GetCellSize(grid, _terrainCellFillRatio, _terrainCellHeight);
                    Gizmos.DrawCube(center, size);
                }
            }
        }

        private Vector3 GetCellCenter(ElementFieldGrid grid, Vector2Int coord, float yOffset, float height)
        {
            return grid.CellToWorld(coord) + grid.FieldSpace.up * (yOffset + height * 0.5f);
        }

        private Vector3 GetCellSize(ElementFieldGrid grid, float fillRatio, float height)
        {
            Vector2 cellSize = grid.CellWorldSize * fillRatio;
            return new Vector3(cellSize.x, height, cellSize.y);
        }

        private Color GetElementColor(ElementType element)
        {
            return element switch
            {
                ElementType.Fire => _fireColor,
                ElementType.Ice => _iceColor,
                ElementType.Poison => _poisonColor,
                ElementType.Lightning => _lightningColor,
                ElementType.Wind => _windColor,
                ElementType.Water => _waterColor,
                _ => _inactiveColor
            };
        }

        private Color GetTerrainColor(TerrainElementType terrain)
        {
            return terrain switch
            {
                TerrainElementType.Grass => _grassTerrainColor,
                TerrainElementType.Sand => _sandTerrainColor,
                TerrainElementType.Water => _waterTerrainColor,
                TerrainElementType.Ice => _iceTerrainColor,
                TerrainElementType.Mud => _mudTerrainColor,
                TerrainElementType.Rock => _rockTerrainColor,
                _ => _noneTerrainColor
            };
        }
    }
}
