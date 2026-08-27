using System.Text;
using Shield_Shot.GameplayCore.Weapon.Projectile;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Field
{
    public class ElementGridSystemValidator : MonoBehaviour
    {
        [Header("Run")]
        [SerializeField] private bool _validateOnAwake = true;
        [SerializeField] private bool _validateOnStart;
        [SerializeField] private bool _includeInactive = true;
        [SerializeField] private bool _skipIfNestedUnderValidator = true;

        [Header("Required Field Parts")]
        [SerializeField] private bool _requireFieldGrid = true;
        [SerializeField] private bool _requireTerrain = true;
        [SerializeField] private bool _requireTerrainPainter = true;
        [SerializeField] private bool _requireSpawnPointProvider = true;
        [SerializeField] private bool _requireEffectSystem = true;

        [Header("Optional Parts")]
        [SerializeField] private bool _checkVisualController = true;
        [SerializeField] private bool _checkBoundaryBuilder = true;
        [SerializeField] private bool _checkCameraPresetController = true;
        [SerializeField] private bool _checkWeaponCorePlacer = true;
        [SerializeField] private bool _checkProjectileManager;

        [Header("Warnings")]
        [SerializeField] private bool _warnAboutLegacyTerrainProvider = true;
        [SerializeField] private bool _warnAboutEnabledDebugTools = true;

        private void Awake()
        {
            if (_validateOnAwake)
            {
                TryValidateRuntimeSetup();
            }
        }

        private void Start()
        {
            if (_validateOnStart)
            {
                TryValidateRuntimeSetup();
            }
        }

        [ContextMenu("Validate Runtime Setup")]
        public void ValidateRuntimeSetup()
        {
            TryValidateRuntimeSetup();
        }

        public bool TryValidateRuntimeSetup()
        {
            if (_skipIfNestedUnderValidator && HasParentValidator())
            {
                return true;
            }

            ValidationReport report = new ValidationReport(gameObject.name);

            CheckSingleInChildren<ElementFieldGrid>("ElementFieldGrid", _requireFieldGrid, report);
            CheckSingleInChildren<Terrain>("Terrain", _requireTerrain, report);
            CheckSingleInChildren<ArenaTerrainPainter>("ArenaTerrainPainter", _requireTerrainPainter, report);
            CheckSingleInChildren<ArenaSpawnPointProvider>("ArenaSpawnPointProvider", _requireSpawnPointProvider, report);
            CheckSingleInChildren<ElementFieldEffectSystem>("ElementFieldEffectSystem", _requireEffectSystem, report);

            CheckSingleInChildren<ElementFieldVisualController>("ElementFieldVisualController", false, report, _checkVisualController);
            CheckSingleInChildren<ArenaBoundaryBuilder>("ArenaBoundaryBuilder", false, report, _checkBoundaryBuilder);
            CheckSingleInChildren<ArenaCameraPresetController>("ArenaCameraPresetController", false, report, _checkCameraPresetController);
            CheckSingleInChildren<ArenaWeaponCoreSpawner>("ArenaWeaponCoreSpawner", false, report, _checkWeaponCorePlacer);

            ValidateTerrainData(report);
            ValidateSpawnPose(report);
            ValidateLegacyTerrainProvider(report);
            ValidateDebugTools(report);
            ValidateProjectileManager(report);

            report.Log(this);
            return report.ErrorCount == 0;
        }

        private bool HasParentValidator()
        {
            Transform parent = transform.parent;

            while (parent != null)
            {
                if (parent.GetComponent<ElementGridSystemValidator>() != null)
                {
                    return true;
                }

                parent = parent.parent;
            }

            return false;
        }

        private void CheckSingleInChildren<T>(
            string label,
            bool required,
            ValidationReport report,
            bool shouldCheck = true)
            where T : Component
        {
            if (!shouldCheck)
            {
                return;
            }

            T[] components = GetComponentsInChildren<T>(_includeInactive);

            if (components.Length == 0)
            {
                if (required)
                {
                    report.Error($"{label} is missing.");
                }
                else
                {
                    report.Warning($"{label} is not present. This is optional.");
                }

                return;
            }

            if (components.Length > 1)
            {
                report.Warning($"{label} count is {components.Length}. Expected one under this system root.");
            }

            Behaviour behaviour = components[0] as Behaviour;
            if (behaviour != null && !behaviour.enabled)
            {
                report.Warning($"{label} exists but is disabled: {components[0].name}");
            }

            report.Ok($"{label}: {components[0].name}");
        }

        private void ValidateTerrainData(ValidationReport report)
        {
            Terrain terrain = GetComponentInChildren<Terrain>(_includeInactive);

            if (terrain == null)
            {
                return;
            }

            if (terrain.terrainData == null)
            {
                report.Error("Terrain exists but TerrainData is missing.");
                return;
            }

            report.Ok($"TerrainData: {terrain.terrainData.name}, Size: {terrain.terrainData.size}");
        }

        private void ValidateSpawnPose(ValidationReport report)
        {
            ArenaSpawnPointProvider spawnProvider = GetComponentInChildren<ArenaSpawnPointProvider>(_includeInactive);

            if (spawnProvider == null)
            {
                return;
            }

            if (spawnProvider.TryGetPlayerSpawnPose(out Pose pose))
            {
                report.Ok($"Player spawn pose resolved: {pose.position}");
                return;
            }

            report.Warning("Player spawn pose could not be resolved. Check ElementFieldGrid and player spawn cell.");
        }

        private void ValidateLegacyTerrainProvider(ValidationReport report)
        {
            if (!_warnAboutLegacyTerrainProvider)
            {
                return;
            }

            ElementFieldTerrainProvider provider = GetComponentInChildren<ElementFieldTerrainProvider>(_includeInactive);

            if (provider != null)
            {
                report.Warning("ElementFieldTerrainProvider is present. Treat it as an optional area override, not the main terrain pipeline.");
            }
        }

        private void ValidateDebugTools(ValidationReport report)
        {
            if (!_warnAboutEnabledDebugTools)
            {
                return;
            }

            ElementFieldPaintDebugTool paintDebugTool = GetComponentInChildren<ElementFieldPaintDebugTool>(_includeInactive);
            ElementFieldDebugView debugView = GetComponentInChildren<ElementFieldDebugView>(_includeInactive);

            if (paintDebugTool != null && paintDebugTool.enabled)
            {
                report.Warning("ElementFieldPaintDebugTool is enabled. Disable it for production scenes.");
            }

            if (debugView != null && debugView.enabled)
            {
                report.Warning("ElementFieldDebugView is enabled. This is useful for development, but noisy for production scenes.");
            }
        }

        private void ValidateProjectileManager(ValidationReport report)
        {
            if (!_checkProjectileManager)
            {
                return;
            }

            ProjectileManager projectileManager = ProjectileManager.Instance;

            if (projectileManager == null)
            {
                projectileManager = FindFirstObjectByType<ProjectileManager>();
            }

            if (projectileManager == null)
            {
                report.Warning("ProjectileManager is missing in the scene. Weapon firing will fail until one exists.");
                return;
            }

            report.Ok($"ProjectileManager: {projectileManager.name}");
        }

        private sealed class ValidationReport
        {
            private readonly string _systemName;
            private readonly StringBuilder _builder = new StringBuilder();

            public int ErrorCount { get; private set; }
            public int WarningCount { get; private set; }

            public ValidationReport(string systemName)
            {
                _systemName = systemName;
            }

            public void Ok(string message)
            {
                _builder.AppendLine($"[OK] {message}");
            }

            public void Warning(string message)
            {
                WarningCount++;
                _builder.AppendLine($"[WARN] {message}");
            }

            public void Error(string message)
            {
                ErrorCount++;
                _builder.AppendLine($"[ERROR] {message}");
            }

            public void Log(Object context)
            {
                string header = $"[ElementGridSystemValidator] {_systemName} validation complete. Errors: {ErrorCount}, Warnings: {WarningCount}";
                string body = _builder.ToString();

                if (ErrorCount > 0)
                {
                    Debug.LogError($"{header}\n{body}", context);
                    return;
                }

                if (WarningCount > 0)
                {
                    Debug.LogWarning($"{header}\n{body}", context);
                    return;
                }

                Debug.Log($"{header}\n{body}", context);
            }
        }
    }
}
