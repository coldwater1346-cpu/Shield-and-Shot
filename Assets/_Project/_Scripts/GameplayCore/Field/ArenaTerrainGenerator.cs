using UnityEngine;

namespace Shield_Shot.GameplayCore.Field
{
    public class ArenaTerrainGenerator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ElementFieldGrid _fieldGrid;
        [SerializeField] private ArenaThemeSO _theme;

        private void Reset()
        {
            _fieldGrid = FindFirstObjectByType<ElementFieldGrid>();
        }

        [ContextMenu("Generate Terrain Data")]
        public void GenerateTerrainData()
        {
            ElementFieldGrid grid = ResolveGrid();

            if (grid == null)
            {
                Debug.LogWarning("[ArenaTerrainGenerator] ElementFieldGrid is missing.");
                return;
            }

            if (_theme == null)
            {
                Debug.LogWarning("[ArenaTerrainGenerator] ArenaThemeSO is missing.");
                return;
            }

            grid.FillTerrain(_theme.BaseTerrain);

            Debug.Log(
                $"[ArenaTerrainGenerator] Generated terrain data. " +
                $"Theme: {_theme.ThemeName}, BaseTerrain: {_theme.BaseTerrain}"
            );
        }

        private ElementFieldGrid ResolveGrid()
        {
            if (_fieldGrid == null)
            {
                _fieldGrid = ElementFieldGrid.Instance != null
                    ? ElementFieldGrid.Instance
                    : FindFirstObjectByType<ElementFieldGrid>();
            }

            return _fieldGrid;
        }
    }
}