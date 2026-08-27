using System;

namespace Shield_Shot.GameplayCore.Monster.Difficulty
{
    /// <summary>
    /// 몬스터의 가벼운 정체성 태그. 특성 규칙이 "어떤 몬스터에게" 주입될지 거를 때 쓴다.
    ///
    /// 몬스터 SO에는 이제 특성 리스트를 다 채울 필요 없이 태그 하나만 켜두면 된다.
    /// 예) 슬라임 = Melee | Small | Splittable,  유령 = Ranged | Undead
    ///     "분열" 규칙은 requiredTags=Splittable → 슬라임에만,  "부활"은 Undead에만 주입.
    ///
    /// [Flags] 이므로 한 몬스터가 여러 태그를 동시에 가질 수 있다.
    /// 새 태그가 필요하면 2의 거듭제곱 값으로 항목만 추가하면 된다.
    /// </summary>
    [Flags]
    public enum MonsterTag
    {
        None       = 0,
        Melee      = 1 << 0,
        Ranged     = 1 << 1,
        Small      = 1 << 2,
        Large      = 1 << 3,
        Fast       = 1 << 4,
        Splittable = 1 << 5,   // 분열 가능
        Undead     = 1 << 6,   // 부활 대상
        Flying     = 1 << 7,
        Elite      = 1 << 8,
        // ... 필요 시 1 << 9 부터 계속 추가
    }

    /// <summary>특성이 행동 트리의 어느 슬롯으로 들어가는지.</summary>
    public enum TraitSlot
    {
        Behavior,   // 생존 중 행동 (회피 등)
        Death,      // 사망 처리 (부활/분열 등)
    }
}
