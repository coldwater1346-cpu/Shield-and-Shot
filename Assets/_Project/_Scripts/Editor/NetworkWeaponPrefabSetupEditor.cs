using Shield_Shot.GameplayCore.Weapon;
using Shield_Shot.GameplayCore.Weapon.Aim;
using Shield_Shot.GameplayCore.Weapon.Charge;
using Shield_Shot.GameplayCore.Weapon.Core;
using Shield_Shot.GameplayCore.Weapon.Projectile;
using Shield_Shot.GameplayCore.Network.Pvp;
using Fusion;
using UnityEditor;
using UnityEngine;

namespace Shield_Shot.EditorTools
{
    public static class NetworkWeaponPrefabSetupEditor
    {
        private const string NetworkPrefabFolder = "Assets/_Project/Resources/Prefabs/Network";
        private const string NetworkWeaponCorePrefabPath = "Assets/_Project/Resources/Prefabs/WeaponCore_Network.prefab";
        private const string NetworkArrowPrefabPath = "Assets/_Project/_Prefabs/Weapon/Network/Arrow_Network.prefab";

        [MenuItem("Shield Shot/PvP/Setup Network Weapon Prefabs")]
        public static void SetupNetworkWeaponPrefabs()
        {
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { NetworkPrefabFolder });
            int changedCount = 0;

            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefabRoot = PrefabUtility.LoadPrefabContents(path);
                bool changed = false;

                try
                {
                    GameObject weaponObject = ResolveWeaponObject(prefabRoot);
                    if (weaponObject == null)
                    {
                        Debug.LogWarning($"[NetworkWeaponPrefabSetup] Weapon object not found: {path}");
                        continue;
                    }

                    changed |= EnsureComponent<WeaponAimController>(weaponObject);
                    changed |= EnsureComponent<WeaponRotationController>(weaponObject);
                    changed |= EnsureComponent<WeaponChargeController>(weaponObject);
                    changed |= EnsureComponent<LineRenderer>(weaponObject);
                    changed |= EnsureComponent<AimLineRenderer>(weaponObject);
                    changed |= EnsureComponent<ProjectileModifierApplier>(weaponObject);
                    changed |= EnsureComponent<StatCalculator>(weaponObject);
                    changed |= EnsureComponent<AimLineMarker>(weaponObject);
                    changed |= DisableLocalProjectileShooter(weaponObject);

                    if (changed)
                    {
                        PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
                        changedCount++;
                    }
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(prefabRoot);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[NetworkWeaponPrefabSetup] Completed. Changed {changedCount}/{prefabGuids.Length} prefabs.");
        }

        [MenuItem("Shield Shot/PvP/Setup WeaponCore Network Prefab")]
        public static void SetupWeaponCoreNetworkPrefab()
        {
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(NetworkWeaponCorePrefabPath);
            bool changed = false;

            try
            {
                changed |= EnsureComponent<NetworkObject>(prefabRoot);
                changed |= EnsureComponent<PvpWeaponActorIdentity>(prefabRoot);
                changed |= EnsureComponent<PvpWeaponHealth>(prefabRoot);
                changed |= EnsureComponent<PvpWeaponHitTarget>(prefabRoot);
                changed |= EnsureComponent<PlayerStatus>(prefabRoot);
                changed |= EnsureComponent<StatCalculator>(prefabRoot);
                changed |= EnsureComponent<NetworkProjectileFireHandler>(prefabRoot);
                changed |= EnsureComponent<NetworkWeaponManager>(prefabRoot);
                changed |= EnsureComponent<PvpProjectileAugmentSnapshotProvider>(prefabRoot);
                changed |= EnsureComponent<NetworkLocalPlayerStatusBinder>(prefabRoot);

                changed |= ConfigureRootPlayerStatus(prefabRoot);
                changed |= ConfigureNetworkProjectileFireHandler(prefabRoot);
                changed |= ConfigureNetworkWeaponManager(prefabRoot);
                changed |= DisableLocalPlayerRuntimeComponents(prefabRoot);

                if (changed)
                {
                    PrefabUtility.SaveAsPrefabAsset(prefabRoot, NetworkWeaponCorePrefabPath);
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[NetworkWeaponPrefabSetup] WeaponCore_Network setup complete. Changed: {changed}");
        }

        private static GameObject ResolveWeaponObject(GameObject prefabRoot)
        {
            WeaponBase weapon = prefabRoot.GetComponentInChildren<WeaponBase>(true);
            if (weapon != null)
            {
                return weapon.gameObject;
            }

            if (prefabRoot.name.StartsWith("Bow"))
            {
                prefabRoot.AddComponent<BowWeapon>();
                return prefabRoot;
            }

            return null;
        }

        private static bool EnsureComponent<T>(GameObject target) where T : Component
        {
            if (target.GetComponent<T>() != null)
            {
                return false;
            }

            target.AddComponent<T>();
            return true;
        }

        private static bool DisableLocalProjectileShooter(GameObject weaponObject)
        {
            ProjectileShooter shooter = weaponObject.GetComponent<ProjectileShooter>();
            if (shooter == null)
            {
                return false;
            }

            SerializedObject serializedShooter = new SerializedObject(shooter);
            SerializedProperty allowLocalFire = serializedShooter.FindProperty("_allowLocalFire");
            if (allowLocalFire == null || !allowLocalFire.boolValue)
            {
                return false;
            }

            allowLocalFire.boolValue = false;
            serializedShooter.ApplyModifiedPropertiesWithoutUndo();
            return true;
        }

        private static bool ConfigureRootPlayerStatus(GameObject prefabRoot)
        {
            PlayerStatus playerStatus = prefabRoot.GetComponent<PlayerStatus>();
            if (playerStatus == null)
            {
                return false;
            }

            bool changed = false;
            SerializedObject serializedStatus = new SerializedObject(playerStatus);
            changed |= SetBool(serializedStatus, "_registerAsLocalPlayer", false);
            changed |= SetBool(serializedStatus, "_useStageFallOnDeath", false);
            if (changed)
            {
                serializedStatus.ApplyModifiedPropertiesWithoutUndo();
            }

            return changed;
        }

        private static bool ConfigureNetworkProjectileFireHandler(GameObject prefabRoot)
        {
            NetworkProjectileFireHandler fireHandler = prefabRoot.GetComponent<NetworkProjectileFireHandler>();
            NetworkObject arrowPrefab = AssetDatabase.LoadAssetAtPath<NetworkObject>(NetworkArrowPrefabPath);
            if (fireHandler == null || arrowPrefab == null)
            {
                return false;
            }

            SerializedObject serializedFireHandler = new SerializedObject(fireHandler);
            SerializedProperty projectilePrefab = serializedFireHandler.FindProperty("_projectilePrefab");
            if (projectilePrefab == null || projectilePrefab.objectReferenceValue == arrowPrefab)
            {
                return false;
            }

            projectilePrefab.objectReferenceValue = arrowPrefab;
            serializedFireHandler.ApplyModifiedPropertiesWithoutUndo();
            return true;
        }

        private static bool ConfigureNetworkWeaponManager(GameObject prefabRoot)
        {
            NetworkWeaponManager weaponManager = prefabRoot.GetComponent<NetworkWeaponManager>();
            if (weaponManager == null)
            {
                return false;
            }

            bool changed = false;
            SerializedObject serializedWeaponManager = new SerializedObject(weaponManager);
            changed |= SetObject(serializedWeaponManager, "_weaponMountPoint", prefabRoot.transform);
            changed |= SetString(serializedWeaponManager, "_networkWeaponResourcesFolder", "Prefabs/Network");
            changed |= SetString(serializedWeaponManager, "_networkWeaponNameSuffix", string.Empty);
            changed |= SetBool(serializedWeaponManager, "_allowLocalPrefabFallback", false);
            if (changed)
            {
                serializedWeaponManager.ApplyModifiedPropertiesWithoutUndo();
            }

            return changed;
        }

        private static bool DisableLocalPlayerRuntimeComponents(GameObject prefabRoot)
        {
            Transform player = prefabRoot.transform.Find("Player");
            if (player == null)
            {
                return false;
            }

            bool changed = false;
            changed |= DisableComponent<Shield_Shot.GameplayCore.Weapon.WeaponManager>(player.gameObject);
            changed |= DisableComponent<ProjectileObjectPool>(player.gameObject);
            changed |= DisableComponent<PlayerStatus>(player.gameObject);
            return changed;
        }

        private static bool DisableComponent<T>(GameObject target) where T : UnityEngine.Behaviour
        {
            T component = target.GetComponent<T>();
            if (component == null || !component.enabled)
            {
                return false;
            }

            component.enabled = false;
            return true;
        }

        private static bool SetBool(SerializedObject serializedObject, string propertyName, bool value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.boolValue == value)
            {
                return false;
            }

            property.boolValue = value;
            return true;
        }

        private static bool SetString(SerializedObject serializedObject, string propertyName, string value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.stringValue == value)
            {
                return false;
            }

            property.stringValue = value;
            return true;
        }

        private static bool SetObject(SerializedObject serializedObject, string propertyName, Object value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.objectReferenceValue == value)
            {
                return false;
            }

            property.objectReferenceValue = value;
            return true;
        }
    }
}
