using System.Collections.Generic;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Field
{
    [CreateAssetMenu(
        menuName = "Shield Shot/Field/Arena Theme",
        fileName = "ArenaThemeSO"
    )]
    public class ArenaThemeSO : ScriptableObject
    {
        [Header("Theme")]
        [SerializeField] private string _themeName = "Grassland";
        [SerializeField] private TerrainElementType _baseTerrain = TerrainElementType.Grass;

        [Header("Base Visual")]
        [SerializeField] private GameObject[] _baseTilePrefabs;

        public string ThemeName => _themeName;
        public TerrainElementType BaseTerrain => _baseTerrain;
        public IReadOnlyList<GameObject> BaseTilePrefabs => _baseTilePrefabs;
    }
}