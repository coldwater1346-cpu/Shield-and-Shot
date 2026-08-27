using Shield_Shot.GameplayCore.Field;
using UnityEngine;


namespace Shield_Shot.GameplayCore.Monster.Stage
{
    /// 아레나 씬 구성: 지형 · 경계벽 · 랜덤벽 · 카메라 프리셋.
    public class ArenaInitializer : MonoBehaviour
    {
        [SerializeField] private ArenaTerrainPainter _terrainPainter;
        [SerializeField] private ArenaBoundaryBuilder _boundaryBuilder;
        [SerializeField] private ArenaRandomReflectWallBuilder _randomWallBuilder;
        [SerializeField] private ArenaCameraPresetController _cameraPreset;

        public void Initialize()
        {
            ResolveReferences();

            if (_terrainPainter != null)
            {
                _terrainPainter.ResetArenaTerrain();
                _terrainPainter.GenerateThemeTerrain();
            }
            else Debug.LogWarning("[Arena] TerrainPainter 없음 → terrain 스킵");

            if (_boundaryBuilder != null)
                _boundaryBuilder.BuildWalls();

            if (_randomWallBuilder == null && _boundaryBuilder != null)
            {
                _randomWallBuilder = _boundaryBuilder.GetComponent<ArenaRandomReflectWallBuilder>()
                    ?? _boundaryBuilder.gameObject.AddComponent<ArenaRandomReflectWallBuilder>();
            }

            if (_randomWallBuilder != null)
            {
                _randomWallBuilder.ConfigureFromBoundaryBuilder(_boundaryBuilder);
                _randomWallBuilder.SetGeneratedWallLayerName("FieldWall");
                _randomWallBuilder.BuildRandomWalls();
            }
        }

        public void ApplyCameraPreset()
        {
            ResolveReferences();
            if (_cameraPreset != null) _cameraPreset.ApplyCameraPreset();
            else Debug.LogWarning("[Arena] CameraPresetController 없음 → preset 스킵");
        }

        public void ValidateTheme()
        {
            if (_terrainPainter != null) _terrainPainter.ValidateTheme();
        }

        private void ResolveReferences()
        {
            if (_terrainPainter == null) _terrainPainter = FindFirstObjectByType<ArenaTerrainPainter>();
            if (_boundaryBuilder == null) _boundaryBuilder = FindFirstObjectByType<ArenaBoundaryBuilder>();
            if (_randomWallBuilder == null) _randomWallBuilder = FindFirstObjectByType<ArenaRandomReflectWallBuilder>();
            if (_cameraPreset == null) _cameraPreset = FindFirstObjectByType<ArenaCameraPresetController>();
        }
    }
}