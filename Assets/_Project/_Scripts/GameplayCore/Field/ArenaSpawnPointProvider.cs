using System.Collections.Generic;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Field
{
    public class ArenaSpawnPointProvider : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ElementFieldGrid _fieldGrid;
        [SerializeField] private Terrain _terrain;

        [Header("Spawn Cells")]
        [SerializeField] private Vector2Int _playerSpawnCell = new Vector2Int(4, 2);
        [SerializeField] private Vector2Int[] _monsterSpawnCells =
        {
            new Vector2Int(4, 12),
            new Vector2Int(3, 13),
            new Vector2Int(5, 13)
        };
        [SerializeField] private Vector2Int _bossSpawnCell = new Vector2Int(4, 14);

        [Header("Placement")]
        [SerializeField] private bool _clampCellsToGrid = true;
        [SerializeField] private bool _snapToTerrainHeight = true;
        [SerializeField] private bool _alignRotationToField = true;
        [SerializeField] private float _spawnYOffset;

        [Header("Gizmos")]
        [SerializeField] private bool _drawGizmos = true;
        [SerializeField] private float _gizmoRadius = 0.25f;

        public Vector2Int PlayerSpawnCell => ResolveCell(_playerSpawnCell);
        public Vector2Int BossSpawnCell => ResolveCell(_bossSpawnCell);

        private void Reset()
        {
            _fieldGrid = FindFirstObjectByType<ElementFieldGrid>();
            _terrain = FindFirstObjectByType<Terrain>();
        }

        public Vector3 GetPlayerSpawnPosition()
        {
            return GetCellWorldPosition(_playerSpawnCell);
        }

        public bool TryGetPlayerSpawnPose(out Pose pose)
        {
            return TryGetCellWorldPose(_playerSpawnCell, out pose);
        }

        public Vector3 GetBossSpawnPosition()
        {
            return GetCellWorldPosition(_bossSpawnCell);
        }

        public bool TryGetBossSpawnPose(out Pose pose)
        {
            return TryGetCellWorldPose(_bossSpawnCell, out pose);
        }

        public IReadOnlyList<Vector3> GetMonsterSpawnPositions()
        {
            List<Vector3> positions = new List<Vector3>(_monsterSpawnCells.Length);

            for (int i = 0; i < _monsterSpawnCells.Length; i++)
            {
                positions.Add(GetCellWorldPosition(_monsterSpawnCells[i]));
            }

            return positions;
        }

        public bool TryGetMonsterSpawnPose(int index, out Pose pose)
        {
            if (_monsterSpawnCells == null || _monsterSpawnCells.Length == 0)
            {
                pose = default;
                return false;
            }

            int safeIndex = Mathf.Abs(index) % _monsterSpawnCells.Length;
            return TryGetCellWorldPose(_monsterSpawnCells[safeIndex], out pose);
        }

        public bool TryGetRandomMonsterSpawnPose(out Pose pose)
        {
            if (_monsterSpawnCells == null || _monsterSpawnCells.Length == 0)
            {
                pose = default;
                return false;
            }

            return TryGetMonsterSpawnPose(Random.Range(0, _monsterSpawnCells.Length), out pose);
        }

        public Vector3 GetCellWorldPosition(Vector2Int cell)
        {
            if (TryGetCellWorldPose(cell, out Pose pose))
            {
                return pose.position;
            }

            return transform.position + Vector3.up * _spawnYOffset;
        }

        public bool TryGetCellWorldPose(Vector2Int cell, out Pose pose)
        {
            ElementFieldGrid grid = ResolveFieldGrid();

            if (grid == null)
            {
                pose = default;
                return false;
            }

            Vector2Int resolvedCell = ResolveCell(cell);

            if (!grid.IsValidCell(resolvedCell))
            {
                pose = default;
                return false;
            }

            Vector3 worldPosition = grid.CellToWorld(resolvedCell);
            Quaternion rotation = _alignRotationToField ? grid.FieldSpace.rotation : transform.rotation;

            if (_snapToTerrainHeight && _terrain != null)
            {
                worldPosition.y = _terrain.transform.position.y + _terrain.SampleHeight(worldPosition);
                worldPosition += Vector3.up * _spawnYOffset;
            }
            else
            {
                worldPosition += grid.FieldSpace.up * _spawnYOffset;
            }

            pose = new Pose(worldPosition, rotation);
            return true;
        }

        private Vector2Int ResolveCell(Vector2Int cell)
        {
            ElementFieldGrid grid = ResolveFieldGrid();

            if (grid == null || !_clampCellsToGrid)
            {
                return cell;
            }

            return grid.ClampCell(cell);
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
            if (!_drawGizmos)
            {
                return;
            }

            DrawSpawnGizmo(_playerSpawnCell, Color.green);

            for (int i = 0; i < _monsterSpawnCells.Length; i++)
            {
                DrawSpawnGizmo(_monsterSpawnCells[i], Color.red);
            }

            DrawSpawnGizmo(_bossSpawnCell, Color.magenta);
        }

        private void DrawSpawnGizmo(Vector2Int cell, Color color)
        {
            ElementFieldGrid grid = ResolveFieldGrid();

            if (grid == null)
            {
                return;
            }

            Vector3 position = GetCellWorldPosition(cell);
            Gizmos.color = color;
            Gizmos.DrawWireSphere(position, _gizmoRadius);
        }
    }
}
