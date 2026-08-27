using System;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Field
{

    [CreateAssetMenu(
        menuName = "Shield Shot/Field/Arena Terrain Theme",
        fileName = "ArenaTerrainThemeSO"
    )]
    public class ArenaTerrainThemeSO : ScriptableObject
    {
        [Serializable]
        public struct TerrainLayerEntry
        {
            public TerrainElementType Terrain;
            public TerrainLayer Layer;
        }

        [Serializable]
        public struct PondRule
        {
            public bool Enabled;
            [Min(0)] public int MinCount;
            [Min(0)] public int MaxCount;
            [Min(1)] public int MinRadiusCells;
            [Min(1)] public int MaxRadiusCells;
            [Min(0)] public int MinDistanceCells;
        }

        [Header("Ponds")]
        [SerializeField]
        private PondRule _pondRule = new PondRule
        {
            Enabled = true,
            MinCount = 1,
            MaxCount = 3,
            MinRadiusCells = 2,
            MaxRadiusCells = 4,
            MinDistanceCells = 4
        };

        [Header("Theme")]
        [SerializeField] private string _themeName = "Grassland";
        [SerializeField] private TerrainElementType _baseTerrain = TerrainElementType.Grass;

        [Header("Terrain Layers")]
        [SerializeField] private TerrainLayerEntry[] _terrainLayers;

        [Header("Water")]
        [SerializeField] private GameObject _waterSurfacePrefab;
        [SerializeField] private float _waterSurfaceYOffset = 0.03f;

        public string ThemeName => _themeName;
        public TerrainElementType BaseTerrain => _baseTerrain;
        public TerrainLayerEntry[] TerrainLayers => _terrainLayers;
        public GameObject WaterSurfacePrefab => _waterSurfacePrefab;
        public float WaterSurfaceYOffset => _waterSurfaceYOffset;

        public PondRule Pond => _pondRule;
    }
}
