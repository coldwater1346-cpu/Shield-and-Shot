namespace Shield_Shot.GameplayCore.Monster.Difficulty
{
    /// <summary>
    /// 난이도로 스케일링되는 스탯의 종류.
    ///
    /// 스탯을 새로 추가하려면:
    ///   1) 여기에 항목 하나 추가
    ///   2) <see cref="StatBlock"/> 에 필드 + 인덱서 case 하나 추가
    /// 이 두 곳만 고치면 난이도 프로파일/리졸버/스폰 흐름은 그대로 동작한다.
    ///
    /// [주의] Count 는 항상 마지막에 둔다. (배열/루프 길이로 사용)
    /// </summary>
    public enum StatType
    {
        MaxHP = 0,
        MoveSpeed = 1,
        AttackDamage = 2,
        AttackSpeed = 3,   // 공격 간격의 역수 개념. 지금 안 쓰면 프로파일에서 비워두면 됨.
        Defense = 4,       // 피해 감산용. 확장 예시.

        Count               // ← enum 개수. 반드시 마지막.
    }
}
