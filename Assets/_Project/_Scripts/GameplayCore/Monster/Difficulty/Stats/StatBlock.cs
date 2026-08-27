using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.Difficulty
{
    /// <summary>
    /// 계산이 끝난 "최종 스탯 묶음". 몬스터에 실제로 주입되는 값.
    ///
    /// - GC 부담 없는 struct(값 타입). 스폰 때마다 만들어도 힙 할당이 없다.
    /// - 스탯별 필드 + <see cref="StatType"/> 인덱서를 모두 제공하므로
    ///   컴포넌트는 named 접근(block.MaxHP), 리졸버는 제네릭 접근(block[StatType.MaxHP])을 쓴다.
    ///
    /// 스탯 추가 시 여기와 <see cref="StatType"/> 두 곳만 수정한다.
    /// </summary>
    public struct StatBlock
    {
        public float MaxHP;
        public float MoveSpeed;
        public float AttackDamage;
        public float AttackSpeed;
        public float Defense;

        /// <summary>StatType 으로 읽고 쓰는 제네릭 접근자.</summary>
        public float this[StatType type]
        {
            get
            {
                switch (type)
                {
                    case StatType.MaxHP:        return MaxHP;
                    case StatType.MoveSpeed:    return MoveSpeed;
                    case StatType.AttackDamage: return AttackDamage;
                    case StatType.AttackSpeed:  return AttackSpeed;
                    case StatType.Defense:      return Defense;
                    default:                    return 0f;
                }
            }
            set
            {
                switch (type)
                {
                    case StatType.MaxHP:        MaxHP = value;        break;
                    case StatType.MoveSpeed:    MoveSpeed = value;    break;
                    case StatType.AttackDamage: AttackDamage = value; break;
                    case StatType.AttackSpeed:  AttackSpeed = value;  break;
                    case StatType.Defense:      Defense = value;      break;
                }
            }
        }

        /// <summary>인스펙터 편집용 직렬화 컨테이너에서 런타임 struct 로 변환.</summary>
        public static StatBlock From(SerializableStatBlock s) => new StatBlock
        {
            MaxHP = s.maxHP,
            MoveSpeed = s.moveSpeed,
            AttackDamage = s.attackDamage,
            AttackSpeed = s.attackSpeed,
            Defense = s.defense,
        };

        public override string ToString()
            => $"HP:{MaxHP:0.#} SPD:{MoveSpeed:0.##} ATK:{AttackDamage:0.#} ASPD:{AttackSpeed:0.##} DEF:{Defense:0.#}";
    }

    /// <summary>
    /// 인스펙터에서 편집하는 "기본 스탯"(난이도 1, 레벨 1 기준 원본값).
    /// struct 는 [SerializeField] 로 노출하면 편집 가능하지만, 명확성을 위해 별도 타입으로 둔다.
    /// </summary>
    [System.Serializable]
    public struct SerializableStatBlock
    {
        [Tooltip("기본 체력")] public float maxHP;
        [Tooltip("기본 이동속도")] public float moveSpeed;
        [Tooltip("기본 공격력")] public float attackDamage;
        [Tooltip("기본 공격속도(초당 공격 수 등). 안 쓰면 0")] public float attackSpeed;
        [Tooltip("기본 방어력. 안 쓰면 0")] public float defense;

        public StatBlock ToRuntime() => StatBlock.From(this);
    }
}
