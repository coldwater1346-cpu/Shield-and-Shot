using System.Collections.Generic;
using Shield_Shot.GameplayCore.Render;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Field
{
    public sealed class ElementFieldCellVfxPlayer : MonoBehaviour
    {
        private enum FireVfxMode
        {
            PerCell,
            Cluster
        }

        private struct FireCluster
        {
            public Vector2Int Min;
            public Vector2Int Max;
            public Vector2 CenterSum;
            public int Count;

            public Vector2 Center => Count > 0
                ? CenterSum / Count
                : Vector2.zero;
        }

        [Header("References")]
        [SerializeField] private ElementFieldGrid _fieldGrid;

        [Header("Fire")]
        [SerializeField] private FireVfxMode _fireVfxMode = FireVfxMode.PerCell;
        [SerializeField] private VFXType _fireVfxType = VFXType.FireField;
        [SerializeField] private Vector3 _fireVfxEulerAngles = new Vector3(90f, 0f, 0f);
        [SerializeField, Min(0f)] private float _fireVfxYOffset = 0.02f;
        [SerializeField, Min(0.05f)] private float _fireVfxScaleMultiplier = 1f;
        [SerializeField] private bool _keepFireVfxUntilCellCleared = true;
        [SerializeField, Min(0f)] private float _oneShotFireVfxDuration = 1.5f;

        [Header("Wind")]
        [SerializeField] private VFXType _windVfxType = VFXType.WindField;
        [SerializeField] private Vector3 _windVfxEulerAngles = new Vector3(90f, 0f, 0f);
        [SerializeField, Min(0f)] private float _windVfxYOffset = 0.04f;
        [SerializeField, Min(0.05f)] private float _windVfxScaleMultiplier = 1f;

        [Header("Wind Cluster")]
        [SerializeField, Min(0.02f)] private float _windClusterRefreshInterval = 0.1f;
        [SerializeField, Min(1)] private int _windClusterCellBlockSize = 3;
        [SerializeField, Min(0.1f)] private float _windClusterScalePadding = 0.2f;
        [SerializeField] private bool _windClusterScaleByBounds = false;

        [Header("Cluster")]
        [SerializeField, Min(0.02f)] private float _clusterRefreshInterval = 0.1f;
        [SerializeField, Min(1)] private int _clusterCellBlockSize = 3;
        [SerializeField, Min(0.1f)] private float _clusterScalePadding = 0.4f;
        [SerializeField] private bool _clusterScaleByBounds = true;

        private readonly HashSet<Vector2Int> _activeFireCells = new();
        private readonly HashSet<Vector2Int> _activeWindCells = new();
        private readonly Dictionary<Vector2Int, GameObject> _activeFireVfx = new();
        private readonly List<GameObject> _clusterFireVfx = new();
        private readonly List<GameObject> _clusterWindVfx = new();
        private readonly List<FireCluster> _clusterBuffer = new();
        private readonly List<FireCluster> _windClusterBuffer = new();
        private readonly Dictionary<Vector2Int, int> _clusterIndexByBlock = new();
        private readonly Dictionary<Vector2Int, int> _windClusterIndexByBlock = new();
        private readonly Dictionary<GameObject, Vector3> _baseScales = new();

        private bool _clusterDirty;
        private bool _windClusterDirty;
        private float _nextClusterRefreshTime;
        private float _nextWindClusterRefreshTime;

        private void Awake()
        {
            ResolveFieldGrid();
        }

        private void OnEnable()
        {
            ResolveFieldGrid();

            if (_fieldGrid == null)
            {
                Debug.LogWarning("[ElementFieldCellVfxPlayer] ElementFieldGrid is missing.");
                return;
            }

            _fieldGrid.CellChanged += OnCellChanged;
            _fieldGrid.CellCleared += OnCellCleared;
        }

        private void OnDisable()
        {
            if (_fieldGrid != null)
            {
                _fieldGrid.CellChanged -= OnCellChanged;
                _fieldGrid.CellCleared -= OnCellCleared;
            }

            ClearTrackedFireVfx();
            ClearClusterFireVfx();
            ClearClusterWindVfx();
            _activeFireCells.Clear();
            _activeWindCells.Clear();
        }

        private void Update()
        {
            if (_fireVfxMode == FireVfxMode.Cluster &&
                _clusterDirty &&
                Time.time >= _nextClusterRefreshTime)
            {
                RebuildClusterFireVfx();
                _clusterDirty = false;
                _nextClusterRefreshTime = Time.time + _clusterRefreshInterval;
            }

            if (_windClusterDirty &&
                Time.time >= _nextWindClusterRefreshTime)
            {
                RebuildClusterWindVfx();
                _windClusterDirty = false;
                _nextWindClusterRefreshTime = Time.time + _windClusterRefreshInterval;
            }
        }

        private void ResolveFieldGrid()
        {
            if (_fieldGrid != null)
            {
                return;
            }

            _fieldGrid = GetComponent<ElementFieldGrid>();

            if (_fieldGrid == null)
            {
                _fieldGrid = ElementFieldGrid.Instance;
            }
        }

        private void OnCellChanged(Vector2Int coord, ElementFieldCellData data, ElementReactionResult reaction)
        {
            if (data.CurrentElement == ElementType.Fire && data.IsActive)
            {
                HandleFireCellChanged(coord);
            }
            else
            {
                RemoveFireCell(coord);
            }

            if (data.CurrentElement == ElementType.Wind && data.IsActive)
            {
                HandleWindCellChanged(coord);
            }
            else
            {
                RemoveWindCell(coord);
            }
        }

        private void HandleFireCellChanged(Vector2Int coord)
        {
            _activeFireCells.Add(coord);

            if (_fireVfxMode == FireVfxMode.Cluster)
            {
                ReleaseFireVfx(coord);
                _clusterDirty = true;
                return;
            }

            if (_activeFireVfx.ContainsKey(coord))
            {
                return;
            }

            SpawnFireVfx(coord);
        }

        private void HandleWindCellChanged(Vector2Int coord)
        {
            _activeWindCells.Add(coord);
            _windClusterDirty = true;
        }

        private void OnCellCleared(Vector2Int coord, ElementFieldCellData data)
        {
            RemoveFireCell(coord);
            RemoveWindCell(coord);
        }

        private void RemoveFireCell(Vector2Int coord)
        {
            bool removed = _activeFireCells.Remove(coord);
            ReleaseFireVfx(coord);

            if (_fireVfxMode == FireVfxMode.Cluster && removed)
            {
                _clusterDirty = true;
                _nextClusterRefreshTime = 0f;

                if (_activeFireCells.Count == 0)
                {
                    ClearClusterFireVfx();
                }
            }
        }

        private void RemoveWindCell(Vector2Int coord)
        {
            bool removed = _activeWindCells.Remove(coord);

            if (!removed)
            {
                return;
            }

            _windClusterDirty = true;
            _nextWindClusterRefreshTime = 0f;

            if (_activeWindCells.Count == 0)
            {
                ClearClusterWindVfx();
            }
        }

        private void SpawnFireVfx(Vector2Int coord)
        {
            if (_fieldGrid == null || _fireVfxType == VFXType.None)
            {
                return;
            }

            VFXPoolManager vfxPool = VFXPoolManager.EnsureInstance();
            if (vfxPool == null)
            {
                return;
            }

            Vector3 position = _fieldGrid.CellToWorld(coord);
            position += _fieldGrid.FieldSpace.up * _fireVfxYOffset;

            float autoReleaseTime = _keepFireVfxUntilCellCleared ? 0f : _oneShotFireVfxDuration;
            GameObject vfx = vfxPool.SpawnVFX(
                _fireVfxType,
                position,
                Quaternion.Euler(_fireVfxEulerAngles),
                autoReleaseTime
            );

            if (vfx != null)
            {
                RememberBaseScale(vfx);
                ApplyFireVfxScale(vfx, _fireVfxScaleMultiplier, _fireVfxScaleMultiplier);

                if (_keepFireVfxUntilCellCleared)
                {
                    _activeFireVfx[coord] = vfx;
                }
            }
        }

        private void RebuildClusterFireVfx()
        {
            BuildClusters();

            for (int i = 0; i < _clusterBuffer.Count; i++)
            {
                GameObject vfx = GetOrCreateClusterVfx(i);
                if (vfx == null)
                {
                    continue;
                }

                ApplyClusterTransform(vfx.transform, _clusterBuffer[i]);

                if (!vfx.activeSelf)
                {
                    vfx.SetActive(true);
                    PlayParticleSystems(vfx);
                }
            }

            for (int i = _clusterBuffer.Count; i < _clusterFireVfx.Count; i++)
            {
                ReleaseClusterVfx(i);
            }
        }

        private void RebuildClusterWindVfx()
        {
            BuildWindClusters();

            for (int i = 0; i < _windClusterBuffer.Count; i++)
            {
                GameObject vfx = GetOrCreateWindClusterVfx(i);
                if (vfx == null)
                {
                    continue;
                }

                ApplyWindClusterTransform(vfx.transform, _windClusterBuffer[i]);

                if (!vfx.activeSelf)
                {
                    vfx.SetActive(true);
                    PlayParticleSystems(vfx);
                }
            }

            for (int i = _windClusterBuffer.Count; i < _clusterWindVfx.Count; i++)
            {
                ReleaseWindClusterVfx(i);
            }
        }

        private void BuildClusters()
        {
            _clusterBuffer.Clear();
            _clusterIndexByBlock.Clear();

            foreach (Vector2Int coord in _activeFireCells)
            {
                Vector2Int block = new Vector2Int(
                    coord.x / Mathf.Max(1, _clusterCellBlockSize),
                    coord.y / Mathf.Max(1, _clusterCellBlockSize)
                );

                if (!_clusterIndexByBlock.TryGetValue(block, out int clusterIndex))
                {
                    clusterIndex = _clusterBuffer.Count;
                    _clusterIndexByBlock.Add(block, clusterIndex);
                    _clusterBuffer.Add(new FireCluster
                    {
                        Min = coord,
                        Max = coord,
                        CenterSum = Vector2.zero,
                        Count = 0
                    });
                }

                FireCluster cluster = _clusterBuffer[clusterIndex];
                cluster.Count++;
                cluster.CenterSum += new Vector2(coord.x, coord.y);
                cluster.Min = Vector2Int.Min(cluster.Min, coord);
                cluster.Max = Vector2Int.Max(cluster.Max, coord);
                _clusterBuffer[clusterIndex] = cluster;
            }
        }

        private void BuildWindClusters()
        {
            _windClusterBuffer.Clear();
            _windClusterIndexByBlock.Clear();

            foreach (Vector2Int coord in _activeWindCells)
            {
                Vector2Int block = new Vector2Int(
                    coord.x / Mathf.Max(1, _windClusterCellBlockSize),
                    coord.y / Mathf.Max(1, _windClusterCellBlockSize)
                );

                if (!_windClusterIndexByBlock.TryGetValue(block, out int clusterIndex))
                {
                    clusterIndex = _windClusterBuffer.Count;
                    _windClusterIndexByBlock.Add(block, clusterIndex);
                    _windClusterBuffer.Add(new FireCluster
                    {
                        Min = coord,
                        Max = coord,
                        CenterSum = Vector2.zero,
                        Count = 0
                    });
                }

                FireCluster cluster = _windClusterBuffer[clusterIndex];
                cluster.Count++;
                cluster.CenterSum += new Vector2(coord.x, coord.y);
                cluster.Min = Vector2Int.Min(cluster.Min, coord);
                cluster.Max = Vector2Int.Max(cluster.Max, coord);
                _windClusterBuffer[clusterIndex] = cluster;
            }
        }

        private GameObject GetOrCreateClusterVfx(int index)
        {
            while (_clusterFireVfx.Count <= index)
            {
                _clusterFireVfx.Add(null);
            }

            GameObject existing = _clusterFireVfx[index];
            if (existing != null)
            {
                return existing;
            }

            VFXPoolManager vfxPool = VFXPoolManager.EnsureInstance();
            if (vfxPool == null || _fireVfxType == VFXType.None)
            {
                return null;
            }

            GameObject vfx = vfxPool.SpawnVFX(_fireVfxType, transform.position, Quaternion.Euler(_fireVfxEulerAngles), 0f);
            if (vfx != null)
            {
                RememberBaseScale(vfx);
                _clusterFireVfx[index] = vfx;
            }

            return vfx;
        }

        private GameObject GetOrCreateWindClusterVfx(int index)
        {
            while (_clusterWindVfx.Count <= index)
            {
                _clusterWindVfx.Add(null);
            }

            GameObject existing = _clusterWindVfx[index];
            if (existing != null)
            {
                return existing;
            }

            VFXPoolManager vfxPool = VFXPoolManager.EnsureInstance();
            if (vfxPool == null || _windVfxType == VFXType.None)
            {
                return null;
            }

            GameObject vfx = vfxPool.SpawnVFX(_windVfxType, transform.position, Quaternion.Euler(_windVfxEulerAngles), 0f);
            if (vfx != null)
            {
                RememberBaseScale(vfx);
                _clusterWindVfx[index] = vfx;
            }

            return vfx;
        }

        private void ApplyClusterTransform(Transform vfxTransform, FireCluster cluster)
        {
            if (vfxTransform == null || _fieldGrid == null || cluster.Count <= 0)
            {
                return;
            }

            Vector2 center = cluster.Center;
            Vector2Int centerCoord = new Vector2Int(
                Mathf.RoundToInt(center.x),
                Mathf.RoundToInt(center.y)
            );

            Vector3 position = _fieldGrid.CellToWorld(centerCoord);
            Vector2 cellSize = _fieldGrid.CellWorldSize;
            Vector2 centerOffset = new Vector2(
                (center.x - centerCoord.x) * cellSize.x,
                (center.y - centerCoord.y) * cellSize.y
            );

            position += _fieldGrid.FieldSpace.right * centerOffset.x;
            position += _fieldGrid.FieldSpace.forward * centerOffset.y;
            position += _fieldGrid.FieldSpace.up * _fireVfxYOffset;

            vfxTransform.position = position;
            vfxTransform.rotation = Quaternion.Euler(_fireVfxEulerAngles);

            if (_clusterScaleByBounds)
            {
                Vector2Int sizeInCells = cluster.Max - cluster.Min + Vector2Int.one;
                ApplyFireVfxScale(
                    vfxTransform.gameObject,
                    Mathf.Max(1f, sizeInCells.x + _clusterScalePadding) * _fireVfxScaleMultiplier,
                    Mathf.Max(1f, sizeInCells.y + _clusterScalePadding) * _fireVfxScaleMultiplier
                );
            }
            else
            {
                ApplyFireVfxScale(vfxTransform.gameObject, _fireVfxScaleMultiplier, _fireVfxScaleMultiplier);
            }
        }

        private void ApplyWindClusterTransform(Transform vfxTransform, FireCluster cluster)
        {
            if (vfxTransform == null || _fieldGrid == null || cluster.Count <= 0)
            {
                return;
            }

            Vector2 center = cluster.Center;
            Vector2Int centerCoord = new Vector2Int(
                Mathf.RoundToInt(center.x),
                Mathf.RoundToInt(center.y)
            );

            Vector3 position = _fieldGrid.CellToWorld(centerCoord);
            Vector2 cellSize = _fieldGrid.CellWorldSize;
            Vector2 centerOffset = new Vector2(
                (center.x - centerCoord.x) * cellSize.x,
                (center.y - centerCoord.y) * cellSize.y
            );

            position += _fieldGrid.FieldSpace.right * centerOffset.x;
            position += _fieldGrid.FieldSpace.forward * centerOffset.y;
            position += _fieldGrid.FieldSpace.up * _windVfxYOffset;

            vfxTransform.position = position;
            vfxTransform.rotation = Quaternion.Euler(_windVfxEulerAngles);

            if (_windClusterScaleByBounds)
            {
                Vector2Int sizeInCells = cluster.Max - cluster.Min + Vector2Int.one;
                ApplyWindVfxScale(
                    vfxTransform.gameObject,
                    Mathf.Max(1f, sizeInCells.x + _windClusterScalePadding),
                    Mathf.Max(1f, sizeInCells.y + _windClusterScalePadding)
                );
            }
            else
            {
                ApplyWindVfxScale(vfxTransform.gameObject, 1f, 1f);
            }
        }

        private void ReleaseFireVfx(Vector2Int coord)
        {
            if (!_activeFireVfx.TryGetValue(coord, out GameObject vfx))
            {
                return;
            }

            _activeFireVfx.Remove(coord);

            if (vfx == null)
            {
                return;
            }

            VFXPoolManager vfxPool = VFXPoolManager.Instance;
            if (vfxPool != null)
            {
                RestoreBaseScale(vfx);
                vfxPool.ReturnToPool(_fireVfxType, vfx);
            }
            else
            {
                vfx.SetActive(false);
            }
        }

        private void ReleaseWindClusterVfx(int index)
        {
            if (index < 0 || index >= _clusterWindVfx.Count)
            {
                return;
            }

            GameObject vfx = _clusterWindVfx[index];
            _clusterWindVfx[index] = null;

            if (vfx == null)
            {
                return;
            }

            RestoreBaseScale(vfx);

            VFXPoolManager vfxPool = VFXPoolManager.Instance;
            if (vfxPool != null)
            {
                vfxPool.ReturnToPool(_windVfxType, vfx);
            }
            else
            {
                vfx.SetActive(false);
            }
        }

        private void ReleaseClusterVfx(int index)
        {
            if (index < 0 || index >= _clusterFireVfx.Count)
            {
                return;
            }

            GameObject vfx = _clusterFireVfx[index];
            _clusterFireVfx[index] = null;

            if (vfx == null)
            {
                return;
            }

            RestoreBaseScale(vfx);

            VFXPoolManager vfxPool = VFXPoolManager.Instance;
            if (vfxPool != null)
            {
                vfxPool.ReturnToPool(_fireVfxType, vfx);
            }
            else
            {
                vfx.SetActive(false);
            }
        }

        private void ClearTrackedFireVfx()
        {
            if (_activeFireVfx.Count == 0)
            {
                return;
            }

            List<Vector2Int> coords = new(_activeFireVfx.Keys);
            for (int i = 0; i < coords.Count; i++)
            {
                ReleaseFireVfx(coords[i]);
            }
        }

        private void ClearClusterFireVfx()
        {
            for (int i = 0; i < _clusterFireVfx.Count; i++)
            {
                ReleaseClusterVfx(i);
            }

            _clusterFireVfx.Clear();
            _clusterBuffer.Clear();
            _clusterIndexByBlock.Clear();
            _clusterDirty = false;
        }

        private void ClearClusterWindVfx()
        {
            for (int i = 0; i < _clusterWindVfx.Count; i++)
            {
                ReleaseWindClusterVfx(i);
            }

            _clusterWindVfx.Clear();
            _windClusterBuffer.Clear();
            _windClusterIndexByBlock.Clear();
            _windClusterDirty = false;
        }

        private void RememberBaseScale(GameObject vfx)
        {
            if (vfx != null && !_baseScales.ContainsKey(vfx))
            {
                _baseScales.Add(vfx, vfx.transform.localScale);
            }
        }

        private Vector3 GetBaseScale(GameObject vfx)
        {
            if (vfx != null && _baseScales.TryGetValue(vfx, out Vector3 baseScale))
            {
                return baseScale;
            }

            return Vector3.one;
        }

        private void ApplyFireVfxScale(GameObject vfx, float xMultiplier, float zMultiplier)
        {
            if (vfx == null)
            {
                return;
            }

            Vector3 baseScale = GetBaseScale(vfx);
            float scaleMultiplier = Mathf.Max(0.05f, _fireVfxScaleMultiplier);
            vfx.transform.localScale = new Vector3(
                baseScale.x * Mathf.Max(0.05f, xMultiplier),
                baseScale.y * scaleMultiplier,
                baseScale.z * Mathf.Max(0.05f, zMultiplier)
            );
        }

        private void ApplyWindVfxScale(GameObject vfx, float xMultiplier, float zMultiplier)
        {
            if (vfx == null)
            {
                return;
            }

            Vector3 baseScale = GetBaseScale(vfx);
            float scaleMultiplier = Mathf.Max(0.05f, _windVfxScaleMultiplier);
            vfx.transform.localScale = new Vector3(
                baseScale.x * Mathf.Max(0.05f, xMultiplier) * scaleMultiplier,
                baseScale.y * scaleMultiplier,
                baseScale.z * Mathf.Max(0.05f, zMultiplier) * scaleMultiplier
            );
        }

        private void RestoreBaseScale(GameObject vfx)
        {
            if (vfx == null || !_baseScales.TryGetValue(vfx, out Vector3 baseScale))
            {
                return;
            }

            vfx.transform.localScale = baseScale;
        }

        private static void PlayParticleSystems(GameObject vfx)
        {
            if (vfx == null)
            {
                return;
            }

            ParticleSystem[] particleSystems = vfx.GetComponentsInChildren<ParticleSystem>();
            for (int i = 0; i < particleSystems.Length; i++)
            {
                ParticleSystem particleSystem = particleSystems[i];
                particleSystem.Simulate(0f, true, true);
                particleSystem.Play(true);
            }
        }
    }
}
