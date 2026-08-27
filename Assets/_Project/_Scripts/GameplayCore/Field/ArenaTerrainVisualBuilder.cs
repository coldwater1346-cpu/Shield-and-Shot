using System.Collections.Generic;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Field
{
    public class ArenaTerrainVisualBuilder : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ElementFieldGrid _fieldGrid;
        [SerializeField] private ArenaThemeSO _theme;
        [SerializeField] private Transform _visualRoot;

        [Header("Random")]
        [SerializeField] private int _seed = 12345;
        [SerializeField] private bool _randomRotateY = true;

        private readonly List<GameObject> _spawnedVisuals = new();

        private void Reset()
        {
            _fieldGrid = FindFirstObjectByType<ElementFieldGrid>();
            _visualRoot = transform;
        }

        [ContextMenu("Rebuild Terrain Visuals")]
        public void RebuildTerrainVisuals()
        {
            ElementFieldGrid grid = ResolveGrid();

            if (grid == null)
            {
                Debug.LogWarning("[ArenaTerrainVisualBuilder] ElementFieldGrid is missing.");
                return;
            }

            if (_theme == null)
            {
                Debug.LogWarning("[ArenaTerrainVisualBuilder] ArenaThemeSO is missing.");
                return;
            }

            ClearTerrainVisuals();

            IReadOnlyList<GameObject> prefabs = _theme.BaseTilePrefabs;

            if (prefabs == null || prefabs.Count == 0)
            {
                Debug.LogWarning("[ArenaTerrainVisualBuilder] Base tile prefabs are empty.");
                return;
            }

            Random.InitState(_seed);

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

                    if (data.TerrainElement != _theme.BaseTerrain)
                    {
                        continue;
                    }

                    GameObject prefab = PickPrefab(prefabs);

                    if (prefab == null)
                    {
                        continue;
                    }

                    SpawnTile(grid, coord, prefab);
                }
            }

            Debug.Log($"[ArenaTerrainVisualBuilder] Rebuilt terrain visuals. Count: {_spawnedVisuals.Count}");
        }

        [ContextMenu("Clear Terrain Visuals")]
        public void ClearTerrainVisuals()
        {
            for (int i = _spawnedVisuals.Count - 1; i >= 0; i--)
            {
                GameObject spawned = _spawnedVisuals[i];

                if (spawned == null)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    Destroy(spawned);
                }
                else
                {
                    DestroyImmediate(spawned);
                }
            }

            _spawnedVisuals.Clear();
        }

        private void SpawnTile(ElementFieldGrid grid, Vector2Int coord, GameObject prefab)
        {
            Transform parent = _visualRoot != null ? _visualRoot : transform;
            GameObject tile = Instantiate(prefab, parent);

            tile.name = $"TerrainTile_{coord.x}_{coord.y}_{prefab.name}";
            tile.transform.position = grid.CellToWorld(coord);
            tile.transform.rotation = grid.FieldSpace.rotation;

            if (_randomRotateY)
            {
                tile.transform.Rotate(Vector3.up, Random.Range(0, 4) * 90f, Space.World);
            }

            Vector2 cellSize = grid.CellWorldSize;
            tile.transform.localScale = new Vector3(cellSize.x, 1f, cellSize.y);

            _spawnedVisuals.Add(tile);
        }

        private GameObject PickPrefab(IReadOnlyList<GameObject> prefabs)
        {
            if (prefabs == null || prefabs.Count == 0)
            {
                return null;
            }

            return prefabs[Random.Range(0, prefabs.Count)];
        }

        private ElementFieldGrid ResolveGrid()
        {
            if (_fieldGrid == null)
            {
                _fieldGrid = ElementFieldGrid.Instance != null
                    ? ElementFieldGrid.Instance
                    : FindFirstObjectByType<ElementFieldGrid>();
            }

            return _fieldGrid;
        }
    }
}