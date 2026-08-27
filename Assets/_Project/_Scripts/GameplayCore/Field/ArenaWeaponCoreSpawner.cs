using UnityEngine;
using UnityEngine.Serialization;

namespace Shield_Shot.GameplayCore.Field
{
    public class ArenaWeaponCoreSpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ArenaSpawnPointProvider _spawnPointProvider;

        [Header("Weapon Core")]
        [FormerlySerializedAs("_existingWeaponCore")]
        [SerializeField] private Transform _weaponCore;

        [Header("Find Fallback")]
        [SerializeField] private bool _findByTag = true;
        [SerializeField] private string _weaponCoreTag = "Player";
        [SerializeField] private bool _findByName;
        [SerializeField] private string _weaponCoreName = "WeaponCore";

        [Header("Placement")]
        [FormerlySerializedAs("_spawnOnStart")]
        [SerializeField] private bool _placeOnStart = true;
        [SerializeField] private bool _copySpawnRotation = true;

        public Transform WeaponCore => _weaponCore;

        private void Reset()
        {
            _spawnPointProvider = FindFirstObjectByType<ArenaSpawnPointProvider>();
        }

        private void Start()
        {
            if (_placeOnStart)
            {
                PlaceWeaponCoreAtPlayerSpawn();
            }
        }

        [ContextMenu("Place Weapon Core At Player Spawn")]
        public void PlaceWeaponCoreAtPlayerSpawn()
        {
            if (!TryGetSpawnPose(out Pose spawnPose))
            {
                return;
            }

            if (!TryResolveWeaponCore(out Transform weaponCore))
            {
                return;
            }

            ApplyPose(weaponCore, spawnPose);
        }

        public bool TryPlaceWeaponCoreAtPlayerSpawn()
        {
            if (!TryGetSpawnPose(out Pose spawnPose))
            {
                return false;
            }

            if (!TryResolveWeaponCore(out Transform weaponCore))
            {
                return false;
            }

            ApplyPose(weaponCore, spawnPose);
            return true;
        }

        public void SetWeaponCore(Transform weaponCore)
        {
            _weaponCore = weaponCore;
        }

        private bool TryResolveWeaponCore(out Transform weaponCore)
        {
            if (_weaponCore != null)
            {
                weaponCore = _weaponCore;
                return true;
            }

            if (_findByTag && TryFindWeaponCoreByTag(out weaponCore))
            {
                _weaponCore = weaponCore;
                return true;
            }

            if (_findByName && !string.IsNullOrWhiteSpace(_weaponCoreName))
            {
                GameObject found = GameObject.Find(_weaponCoreName);

                if (found != null)
                {
                    weaponCore = found.transform;
                    _weaponCore = weaponCore;
                    return true;
                }
            }

            Debug.LogWarning("[ArenaWeaponCoreSpawner] WeaponCore was not assigned or found.");
            weaponCore = null;
            return false;
        }

        private bool TryFindWeaponCoreByTag(out Transform weaponCore)
        {
            weaponCore = null;

            if (string.IsNullOrWhiteSpace(_weaponCoreTag))
            {
                return false;
            }

            try
            {
                GameObject found = GameObject.FindGameObjectWithTag(_weaponCoreTag);

                if (found == null)
                {
                    return false;
                }

                weaponCore = found.transform;
                return true;
            }
            catch (UnityException exception)
            {
                Debug.LogWarning($"[ArenaWeaponCoreSpawner] Tag lookup failed. Tag: {_weaponCoreTag}, {exception.Message}");
                return false;
            }
        }

        private bool TryGetSpawnPose(out Pose spawnPose)
        {
            if (_spawnPointProvider == null)
            {
                _spawnPointProvider = FindFirstObjectByType<ArenaSpawnPointProvider>();
            }

            if (_spawnPointProvider == null)
            {
                Debug.LogWarning("[ArenaWeaponCoreSpawner] ArenaSpawnPointProvider is missing.");
                spawnPose = default;
                return false;
            }

            if (!_spawnPointProvider.TryGetPlayerSpawnPose(out spawnPose))
            {
                Debug.LogWarning("[ArenaWeaponCoreSpawner] Player spawn pose could not be resolved.");
                return false;
            }

            return true;
        }

        private void ApplyPose(Transform target, Pose spawnPose)
        {
            target.position = spawnPose.position;

            if (_copySpawnRotation)
            {
                target.rotation = spawnPose.rotation;
            }
        }
    }
}
