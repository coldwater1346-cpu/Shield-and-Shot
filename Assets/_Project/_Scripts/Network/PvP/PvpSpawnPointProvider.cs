using Shield_Shot.GameplayCore.Network.Match;
using Shield_Shot.GameplayCore.Field;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Network.Pvp
{
    public sealed class PvpSpawnPointProvider : MonoBehaviour
    {
        [Header("Cell Based Spawn")]
        [SerializeField] private bool _useCellBasedSpawn = true;
        [SerializeField] private ElementFieldGrid _fieldGrid;
        [SerializeField] private Terrain _terrain;
        [SerializeField] private Vector2Int _bottomWeaponSpawnCell = new Vector2Int(13, 4);
        [SerializeField] private Vector2Int _topWeaponSpawnCell = new Vector2Int(13, 43);
        [SerializeField] private bool _snapToTerrainHeight = true;
        [SerializeField] private float _spawnYOffset;

        [Header("Weapon Spawn Points")]
        [SerializeField] private Transform _bottomWeaponSpawnPoint;
        [SerializeField] private Transform _topWeaponSpawnPoint;

        public bool TryGetWeaponSpawnPose(PlayerSide side, out Pose pose)
        {
            if (_useCellBasedSpawn)
            {
                return TryGetCellBasedWeaponSpawnPose(side, out pose);
            }

            Transform spawnPoint = GetWeaponSpawnPoint(side);

            if (spawnPoint == null)
            {
                pose = default;
                return false;
            }

            pose = new Pose(spawnPoint.position, spawnPoint.rotation);
            return true;
        }

        private bool TryGetCellBasedWeaponSpawnPose(PlayerSide side, out Pose pose)
        {
            ElementFieldGrid grid = ResolveFieldGrid();

            if (grid == null)
            {
                pose = default;
                return false;
            }

            Vector2Int cell = side switch
            {
                PlayerSide.Bottom => _bottomWeaponSpawnCell,
                PlayerSide.Top => _topWeaponSpawnCell,
                _ => default
            };

            if (side != PlayerSide.Bottom && side != PlayerSide.Top)
            {
                pose = default;
                return false;
            }

            cell = grid.ClampCell(cell);
            Vector3 position = grid.CellToWorld(cell);

            if (_snapToTerrainHeight && ResolveTerrain() != null)
            {
                position.y = _terrain.transform.position.y + _terrain.SampleHeight(position);
                position += Vector3.up * _spawnYOffset;
            }
            else
            {
                position += Vector3.up * _spawnYOffset;
            }

            Quaternion rotation = side == PlayerSide.Top
                ? Quaternion.Euler(0f, 180f, 0f)
                : Quaternion.identity;

            pose = new Pose(position, rotation);
            Debug.Log(
                $"[PvpSpawnPointProvider] Cell based weapon spawn pose. " +
                $"Side: {side}, Cell: {cell}, Position: {position}, Rotation: {rotation.eulerAngles}, " +
                $"SnapToTerrain: {_snapToTerrainHeight}, SpawnYOffset: {_spawnYOffset}");
            return true;
        }

        private Transform GetWeaponSpawnPoint(PlayerSide side)
        {
            return side switch
            {
                PlayerSide.Bottom => _bottomWeaponSpawnPoint,
                PlayerSide.Top => _topWeaponSpawnPoint,
                _ => null
            };
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

        private Terrain ResolveTerrain()
        {
            if (_terrain == null)
            {
                _terrain = FindFirstObjectByType<Terrain>();
            }

            return _terrain;
        }
    }
}
