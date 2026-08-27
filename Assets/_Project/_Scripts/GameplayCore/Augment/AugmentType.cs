namespace Shield_Shot.GameplayCore.Augment
{
    public enum AugmentType
    {
        None = 0,

        // ── 투사체 특성 ──
        Reflect,        // 벽 반사
        Pierce,         // 관통
        Split,          // 분열
        Explosion,      // 폭발
        ChainLightning, // 연쇄 번개

        // ── 스탯 강화 ──
        DamageUp,       // 데미지 증가
        SpeedUp,        // 속도 증가
        RangeUp,        // 범위 증가
    }
}