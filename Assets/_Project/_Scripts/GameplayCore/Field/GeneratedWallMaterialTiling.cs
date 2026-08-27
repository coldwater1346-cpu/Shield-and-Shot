using UnityEngine;

namespace Shield_Shot.GameplayCore.Field
{
    public sealed class GeneratedWallMaterialTiling : MonoBehaviour
    {
        private static readonly int BaseMapST = Shader.PropertyToID("_BaseMap_ST");
        private static readonly int MainTexST = Shader.PropertyToID("_MainTex_ST");

        [SerializeField, Min(0.01f)] private float _worldUnitsPerTile = 1f;
        [SerializeField] private bool _applyOnStart = true;

        private MaterialPropertyBlock _propertyBlock;

        private void Start()
        {
            if (_applyOnStart)
            {
                Apply();
            }
        }

        public void Apply()
        {
            Apply(transform.lossyScale, _worldUnitsPerTile);
        }

        public void Apply(Vector3 worldSize, float worldUnitsPerTile)
        {
            if (_propertyBlock == null)
            {
                _propertyBlock = new MaterialPropertyBlock();
            }

            float safeUnitsPerTile = Mathf.Max(0.01f, worldUnitsPerTile);
            Vector2 tiling = ResolveTiling(worldSize, safeUnitsPerTile);
            Renderer[] renderers = GetComponentsInChildren<Renderer>();

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer targetRenderer = renderers[i];
                if (targetRenderer == null)
                {
                    continue;
                }

                targetRenderer.GetPropertyBlock(_propertyBlock);

                if (HasProperty(targetRenderer, BaseMapST))
                {
                    _propertyBlock.SetVector(BaseMapST, new Vector4(tiling.x, tiling.y, 0f, 0f));
                }

                if (HasProperty(targetRenderer, MainTexST))
                {
                    _propertyBlock.SetVector(MainTexST, new Vector4(tiling.x, tiling.y, 0f, 0f));
                }

                targetRenderer.SetPropertyBlock(_propertyBlock);
            }
        }

        private static Vector2 ResolveTiling(Vector3 worldSize, float worldUnitsPerTile)
        {
            Vector3 absoluteSize = new Vector3(
                Mathf.Abs(worldSize.x),
                Mathf.Abs(worldSize.y),
                Mathf.Abs(worldSize.z)
            );

            float primaryAxis = Mathf.Max(absoluteSize.x, absoluteSize.z);
            float secondaryAxis = Mathf.Min(absoluteSize.x, absoluteSize.z);

            if (secondaryAxis <= 0.001f)
            {
                secondaryAxis = absoluteSize.y;
            }

            return new Vector2(
                Mathf.Max(1f, primaryAxis / worldUnitsPerTile),
                Mathf.Max(1f, secondaryAxis / worldUnitsPerTile)
            );
        }

        private static bool HasProperty(Renderer targetRenderer, int propertyId)
        {
            Material material = targetRenderer.sharedMaterial;
            return material != null && material.HasProperty(propertyId);
        }
    }
}
