using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Field
{
    public class ArenaCameraPresetController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ElementFieldGrid _fieldGrid;
        [SerializeField] private ArenaBoundaryBuilder _boundaryBuilder;
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private CinemachineCamera _cinemachineCamera;

        [Header("Preset")]
        [SerializeField] private ArenaCameraViewMode _viewMode = ArenaCameraViewMode.TopView;
        [SerializeField] private bool _applyOnStart = true;
        [SerializeField] private bool _keepAppliedInPlayMode = true;

        [Header("Top View")]
        [SerializeField, Min(0.1f)] private float _topViewHeightMultiplier = 1.25f;
        [SerializeField] private bool _includeWallThickness = true;
        [SerializeField, Range(0f, 1f)] private float _visibleWallRatio = 0.3f;
        [SerializeField, Min(0f)] private float _orthographicPadding;
        [SerializeField] private Vector3 _topViewOffset;

        [Header("2.5D View")]
        [SerializeField, Min(0.1f)] private float _angledHeightMultiplier = 0.8f;
        [SerializeField, Min(0f)] private float _angledBackDistanceMultiplier = 0.55f;
        [SerializeField] private Vector3 _angledViewOffset;

        private void Reset()
        {
            _fieldGrid = FindFirstObjectByType<ElementFieldGrid>();
            _boundaryBuilder = FindFirstObjectByType<ArenaBoundaryBuilder>();
            _mainCamera = Camera.main;
            _cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();
        }

        private IEnumerator Start()
        {
            if (!_applyOnStart)
            {
                yield break;
            }

            // If the camera is a child of WeaponCore, another Start callback can move it
            // while placing WeaponCore. Apply after all first-frame placement has finished.
            yield return new WaitForEndOfFrame();
            ApplyPreset();
        }

        private void LateUpdate()
        {
            if (!_keepAppliedInPlayMode || _viewMode != ArenaCameraViewMode.TopView)
            {
                return;
            }

            ApplyTopViewLens();
        }

        [ContextMenu("Apply Camera Preset")]
        public void ApplyPreset()
        {
            ElementFieldGrid grid = ResolveFieldGrid();
            Transform cameraTransform = ResolveCameraTransform();

            if (grid == null || cameraTransform == null)
            {
                Debug.LogWarning("[ArenaCameraPresetController] Field grid or camera is missing.");
                return;
            }

            Transform fieldSpace = grid.FieldSpace;
            Vector3 center = grid.FieldCenter;
            float fieldLongSide = Mathf.Max(grid.FieldWorldSize.x, grid.FieldWorldSize.y);

            Vector3 position = _viewMode == ArenaCameraViewMode.TopView
                ? GetTopViewPosition(fieldSpace, center, fieldLongSide)
                : GetTwoPointFiveDPosition(fieldSpace, center, fieldLongSide);

            if (_viewMode == ArenaCameraViewMode.TopView)
            {
                ApplyTopViewLens(grid);
            }

            cameraTransform.SetPositionAndRotation(
                position,
                Quaternion.LookRotation(center - position, fieldSpace.forward)
            );
        }

        public void ApplyCameraPreset()
        {
            ApplyPreset();
        }

        public void SetViewMode(ArenaCameraViewMode viewMode, bool applyImmediately = true)
        {
            _viewMode = viewMode;

            if (applyImmediately)
            {
                ApplyPreset();
            }
        }

        public void SetKeepAppliedInPlayMode(bool keepApplied)
        {
            _keepAppliedInPlayMode = keepApplied;
        }

        private Vector3 GetTopViewPosition(Transform fieldSpace, Vector3 center, float fieldLongSide)
        {
            return center +
                   fieldSpace.up * (fieldLongSide * _topViewHeightMultiplier) +
                   fieldSpace.TransformDirection(_topViewOffset);
        }

        private Vector3 GetTwoPointFiveDPosition(Transform fieldSpace, Vector3 center, float fieldLongSide)
        {
            return center +
                   fieldSpace.up * (fieldLongSide * _angledHeightMultiplier) -
                   fieldSpace.forward * (fieldLongSide * _angledBackDistanceMultiplier) +
                   fieldSpace.TransformDirection(_angledViewOffset);
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

        private Transform ResolveCameraTransform()
        {
            if (_cinemachineCamera == null)
            {
                _cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();
            }

            if (_cinemachineCamera != null)
            {
                return _cinemachineCamera.transform;
            }

            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }

            return _mainCamera != null ? _mainCamera.transform : null;
        }

        private void ApplyTopViewLens()
        {
            ElementFieldGrid grid = ResolveFieldGrid();

            if (grid == null)
            {
                return;
            }

            ApplyTopViewLens(grid);
        }

        private void ApplyTopViewLens(ElementFieldGrid grid)
        {
            Camera camera = ResolveMainCamera();
            float aspect = camera != null ? camera.aspect : GetFallbackAspect();
            float orthographicSize = CalculateOrthographicSize(GetCameraFitWorldSize(grid), aspect);

            if (_cinemachineCamera == null)
            {
                _cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();
            }

            if (_cinemachineCamera != null)
            {
                LensSettings lens = _cinemachineCamera.Lens;
                lens.OrthographicSize = orthographicSize;
                _cinemachineCamera.Lens = lens;
            }

            if (camera != null)
            {
                camera.orthographic = true;
                camera.orthographicSize = orthographicSize;
            }
        }

        private Vector2 GetCameraFitWorldSize(ElementFieldGrid grid)
        {
            Vector2 fitSize = grid.FieldWorldSize;

            if (!_includeWallThickness)
            {
                return fitSize;
            }

            ArenaBoundaryBuilder boundaryBuilder = ResolveBoundaryBuilder();

            if (boundaryBuilder == null)
            {
                return fitSize;
            }

            float wallThickness = boundaryBuilder.WallThickness;
            return fitSize + Vector2.one * (wallThickness * 2f * _visibleWallRatio);
        }

        private float CalculateOrthographicSize(Vector2 fitWorldSize, float aspect)
        {
            float safeAspect = Mathf.Max(0.01f, aspect);
            float verticalSize = fitWorldSize.y * 0.5f;
            float horizontalSize = fitWorldSize.x / safeAspect * 0.5f;

            return Mathf.Max(verticalSize, horizontalSize) + _orthographicPadding;
        }

        private ArenaBoundaryBuilder ResolveBoundaryBuilder()
        {
            if (_boundaryBuilder == null)
            {
                _boundaryBuilder = FindFirstObjectByType<ArenaBoundaryBuilder>();
            }

            return _boundaryBuilder;
        }

        private Camera ResolveMainCamera()
        {
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }

            return _mainCamera;
        }

        private static float GetFallbackAspect()
        {
            if (Screen.height <= 0)
            {
                return 9f / 16f;
            }

            return (float)Screen.width / Screen.height;
        }
    }
}
