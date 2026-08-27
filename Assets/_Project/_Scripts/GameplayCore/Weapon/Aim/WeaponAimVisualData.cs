using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Aim
{
    [System.Serializable]
    public struct WeaponAimVisualData
    {
        [Header("Line Visuals")]
        [Tooltip("무기 고유의 조준선 머티리얼 (세모 패닝, 레이저 등)")]
        public Material LineMaterial;

        [Tooltip("조준선 시작 두께")]
        public float StartWidth;

        [Tooltip("조준선 끝 두께")]
        public float EndWidth;

        [Header("Texture Tiling")]
        [Tooltip("라인 렌더러의 Texture Mode 설정")]
        public LineTextureMode TextureMode;

        [Tooltip("텍스처 타일링 스케일 (늘어남 방지용 비율)")]
        public Vector2 TextureScale;
    }
}