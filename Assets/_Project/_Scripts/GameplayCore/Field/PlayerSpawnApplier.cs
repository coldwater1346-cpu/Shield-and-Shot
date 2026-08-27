using UnityEngine;

namespace Shield_Shot.GameplayCore.Field
{
    public class PlayerSpawnApplier : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _playerRoot;
        [SerializeField] private CellAnchoredTransform _spawnPoint;

        [Header("Apply")]
        [SerializeField] private bool _applyOnStart = true;
        [SerializeField] private bool _copyRotation = true;

        private void Reset()
        {
            if (_playerRoot == null)
            {
                _playerRoot = transform;
            }

            if (_spawnPoint == null)
            {
                _spawnPoint = FindFirstObjectByType<CellAnchoredTransform>();
            }
        }

        private void Start()
        {
            if (_applyOnStart)
            {
                Apply();
            }
        }

        [ContextMenu("Apply Player Spawn")]
        public void Apply()
        {
            if (_playerRoot == null)
            {
                Debug.LogWarning("[PlayerSpawnApplier] Player root is missing.");
                return;
            }

            if (_spawnPoint == null)
            {
                Debug.LogWarning("[PlayerSpawnApplier] Spawn point is missing.");
                return;
            }

            _spawnPoint.Apply();
            _playerRoot.position = _spawnPoint.WorldPosition;

            if (_copyRotation)
            {
                _playerRoot.rotation = _spawnPoint.transform.rotation;
            }
        }
    }
}
