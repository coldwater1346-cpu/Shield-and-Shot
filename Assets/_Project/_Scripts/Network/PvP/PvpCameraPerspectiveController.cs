using Shield_Shot.GameplayCore.Network.Match;
using Unity.Cinemachine;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Network.Pvp
{
    public sealed class PvpCameraPerspectiveController : MonoBehaviour
    {
        [Header("Camera")]
        [SerializeField] private CinemachineCamera _targetCinemachineCamera;
        [SerializeField] private Camera _targetCamera;

        [Header("Rotation")]
        [SerializeField] private Vector3 _bottomEulerAngles = Vector3.zero;
        [SerializeField] private Vector3 _topEulerAngles = new Vector3(0f, 0f, 180f);

        private const float TopScreenRollDegrees = 180f;

        private PlayerSide _lastAppliedSide = PlayerSide.None;
        private Quaternion _baseArenaRotation;

        public void ApplyPerspective(PlayerSide localSide)
        {
            Transform cameraTransform = ResolveCameraTransform();

            if (cameraTransform == null)
            {
                Debug.LogError("[PvpCameraPerspectiveController] Target camera is missing.");
                return;
            }

            if (localSide != PlayerSide.Bottom && localSide != PlayerSide.Top)
            {
                Debug.LogWarning($"[PvpCameraPerspectiveController] Unsupported side: {localSide}");
                return;
            }

            if (_lastAppliedSide == PlayerSide.None)
            {
                _baseArenaRotation = cameraTransform.rotation;
            }

            if (_lastAppliedSide == localSide)
            {
                return;
            }

            cameraTransform.rotation = _baseArenaRotation;

            if (localSide == PlayerSide.Top)
            {
                cameraTransform.rotation =
                    Quaternion.AngleAxis(TopScreenRollDegrees, cameraTransform.forward) *
                    cameraTransform.rotation;
            }

            _lastAppliedSide = localSide;

            Debug.Log(
                $"[PvpCameraPerspectiveController] Perspective side resolved: {localSide}, " +
                $"Target: {cameraTransform.name}, Forward: {cameraTransform.forward}, " +
                $"LegacyBottom: {_bottomEulerAngles}, LegacyTop: {_topEulerAngles}, " +
                $"Rotation: {cameraTransform.rotation.eulerAngles}");
        }

        private Transform ResolveCameraTransform()
        {
            if (_targetCinemachineCamera == null)
            {
                _targetCinemachineCamera = FindFirstObjectByType<CinemachineCamera>();
            }

            if (_targetCinemachineCamera != null)
            {
                return _targetCinemachineCamera.transform;
            }

            Camera camera = _targetCamera != null ? _targetCamera : Camera.main;
            return camera != null ? camera.transform : null;
        }
    }
}
