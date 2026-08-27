using UnityEngine;

namespace Shield_Shot.GameplayCore.Field
{
    public class ArenaBoundaryBuilder : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ElementFieldGrid _fieldGrid;
        [SerializeField] private Transform _fieldSpace;
        [SerializeField] private Transform _wallRoot;
        [SerializeField] private GameObject _wallPrefab;

        [Header("Wall Settings")]
        [SerializeField, Min(0.01f)] private float _wallThickness = 0.3f;
        [SerializeField, Min(0.01f)] private float _wallHeight = 1.5f;
        [SerializeField] private bool _buildOnAwake = true;
        [SerializeField] private bool _clearExistingGeneratedWalls = true;
        [SerializeField] private bool _overrideGeneratedWallLayer;
        [SerializeField] private string _generatedWallLayerName = "PvpWall";

        [Header("Material Tiling")]
        [SerializeField] private bool _fitGeneratedWallMaterialTiling = true;
        [SerializeField, Min(0.01f)] private float _wallMaterialWorldUnitsPerTile = 1f;

        private const string GeneratedWallPrefix = "GeneratedArenaWall_";

        public float WallThickness => _wallThickness;
        public ElementFieldGrid FieldGrid => _fieldGrid;
        public Transform FieldSpace => _fieldSpace;
        public Transform WallRoot => _wallRoot;
        public GameObject WallPrefab => _wallPrefab;
        public bool OverrideGeneratedWallLayer => _overrideGeneratedWallLayer;
        public string GeneratedWallLayerName => _generatedWallLayerName;
        public bool FitGeneratedWallMaterialTiling => _fitGeneratedWallMaterialTiling;
        public float WallMaterialWorldUnitsPerTile => _wallMaterialWorldUnitsPerTile;

        private void Awake()
        {
            if (_buildOnAwake)
            {
                BuildWalls();
            }
        }

        [ContextMenu("Build Walls")]
        public void BuildWalls()
        {
            ResolveReferences();

            if (_fieldGrid == null)
            {
                Debug.LogWarning("[ArenaBoundaryBuilder] ElementFieldGrid is missing.");
                return;
            }

            if (_fieldSpace == null)
            {
                Debug.LogWarning("[ArenaBoundaryBuilder] FieldSpace is missing.");
                return;
            }

            if (_wallPrefab == null)
            {
                Debug.LogWarning("[ArenaBoundaryBuilder] Wall prefab is missing.");
                return;
            }

            if (_wallRoot == null)
            {
                _wallRoot = _fieldSpace;
            }

            if (_clearExistingGeneratedWalls)
            {
                ClearGeneratedWalls();
            }

            Vector2 fieldSize = _fieldGrid.FieldWorldSize;
            float halfWidth = fieldSize.x * 0.5f;
            float halfHeight = fieldSize.y * 0.5f;
            float offset = _wallThickness * 0.5f;

            CreateWall(
                "Left",
                new Vector3(-halfWidth - offset, _wallHeight * 0.5f, 0f),
                new Vector3(_wallThickness, _wallHeight, fieldSize.y + _wallThickness * 2f)
            );

            CreateWall(
                "Right",
                new Vector3(halfWidth + offset, _wallHeight * 0.5f, 0f),
                new Vector3(_wallThickness, _wallHeight, fieldSize.y + _wallThickness * 2f)
            );

            CreateWall(
                "Bottom",
                new Vector3(0f, _wallHeight * 0.5f, -halfHeight - offset),
                new Vector3(fieldSize.x + _wallThickness * 2f, _wallHeight, _wallThickness)
            );

            CreateWall(
                "Top",
                new Vector3(0f, _wallHeight * 0.5f, halfHeight + offset),
                new Vector3(fieldSize.x + _wallThickness * 2f, _wallHeight, _wallThickness)
            );
        }

        private void ResolveReferences()
        {
            if (_fieldGrid == null)
            {
                _fieldGrid = GetComponent<ElementFieldGrid>();
            }

            if (_fieldGrid == null)
            {
                _fieldGrid = FindFirstObjectByType<ElementFieldGrid>();
            }

            if (_fieldSpace == null && _fieldGrid != null)
            {
                _fieldSpace = _fieldGrid.transform;
            }

            if (_wallRoot == null)
            {
                _wallRoot = _fieldSpace != null ? _fieldSpace : transform;
            }
        }

        private void CreateWall(string suffix, Vector3 localPosition, Vector3 localScale)
        {
            GameObject wall = Instantiate(_wallPrefab, _wallRoot);
            wall.name = $"{GeneratedWallPrefix}{suffix}";
            wall.transform.localPosition = localPosition;
            wall.transform.localRotation = Quaternion.identity;
            wall.transform.localScale = localScale;
            ApplyGeneratedWallLayer(wall);
            ApplyGeneratedWallMaterialTiling(wall, localScale);
        }

        private void ApplyGeneratedWallMaterialTiling(GameObject wall, Vector3 localScale)
        {
            if (!_fitGeneratedWallMaterialTiling || wall == null)
            {
                return;
            }

            GeneratedWallMaterialTiling tiling = wall.GetComponent<GeneratedWallMaterialTiling>();
            if (tiling == null)
            {
                tiling = wall.AddComponent<GeneratedWallMaterialTiling>();
            }

            tiling.Apply(localScale, _wallMaterialWorldUnitsPerTile);
        }

        private void ApplyGeneratedWallLayer(GameObject wall)
        {
            if (!_overrideGeneratedWallLayer || wall == null || string.IsNullOrWhiteSpace(_generatedWallLayerName))
            {
                return;
            }

            int layer = LayerMask.NameToLayer(_generatedWallLayerName);
            if (layer < 0)
            {
                Debug.LogWarning($"[ArenaBoundaryBuilder] Layer not found: {_generatedWallLayerName}");
                return;
            }

            SetLayerRecursively(wall.transform, layer);
        }

        private static void SetLayerRecursively(Transform target, int layer)
        {
            target.gameObject.layer = layer;

            for (int i = 0; i < target.childCount; i++)
            {
                SetLayerRecursively(target.GetChild(i), layer);
            }
        }

        private void ClearGeneratedWalls()
        {
            if (_wallRoot == null)
            {
                return;
            }

            for (int i = _wallRoot.childCount - 1; i >= 0; i--)
            {
                Transform child = _wallRoot.GetChild(i);
                if (!child.name.StartsWith(GeneratedWallPrefix))
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }
    }
}
