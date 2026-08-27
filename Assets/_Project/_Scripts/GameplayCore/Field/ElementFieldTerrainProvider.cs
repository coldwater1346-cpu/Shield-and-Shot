using System.Collections.Generic;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Field
{
    public class ElementFieldTerrainProvider : MonoBehaviour
    {
        [SerializeField] private bool _autoFindAreas = true;
        [SerializeField] private List<ElementFieldTerrainArea> _areas = new();

        private void Awake()
        {
            if (_autoFindAreas)
            {
                RefreshAreas();
            }
        }

        [ContextMenu("Refresh Areas")]
        public void RefreshAreas()
        {
            _areas.Clear();
            GetComponentsInChildren(_areas);
        }


        public TerrainElementType GetTerrain(Vector3 worldPosition)
        {
            TerrainElementType result = TerrainElementType.None;
            int bestPriority = int.MinValue;

            for (int i = 0; i < _areas.Count; i++)
            {
                ElementFieldTerrainArea area = _areas[i];

                if (area == null || area.TerrainElement == TerrainElementType.None)
                {
                    continue;
                }

                if (!area.ContainsWorldPoint(worldPosition))
                {
                    continue;
                }

                if (area.Priority < bestPriority)
                {
                    continue;
                }

                bestPriority = area.Priority;
                result = area.TerrainElement;
            }

            return result;
        }
    }
}