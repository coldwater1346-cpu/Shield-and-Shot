using System.Collections.Generic;
using UnityEngine;
using Shield_Shot.GameplayCore.Monster.BT.Traits;

namespace Shield_Shot.GameplayCore.Monster.Difficulty
{
    /// <summary>
    /// "이 챕터/스테이지에서 몬스터가 배울 수 있는 특성" 규칙 모음.
    ///
    /// 특성을 몬스터 SO마다 다 다는 대신 여기에 한 번 정의하고,
    /// 난이도에 따라 확률로 몬스터에게 '주입'한다. (기존 GatedTrait 의 상위 개념)
    ///
    /// 규칙은 챕터/스테이지 레벨에 배치되므로, 같은 몬스터라도
    /// 챕터가 바뀌면 다른 특성 세트를 얻는다. 밸런싱은 이 에셋만 갈아끼우면 끝.
    /// </summary>
    [CreateAssetMenu(fileName = "TraitRuleSet", menuName = "Monster/Difficulty/Trait Rule Set")]
    public class TraitRuleSetSO : ScriptableObject
    {
        [System.Serializable]
        public struct TraitRule
        {
            public BtTraitSO trait;
            public TraitSlot slot;

            [Tooltip("이 난이도 이상부터 후보")]
            public int minDifficulty;

            [Range(0f, 1f)]
            [Tooltip("부여 확률 (그룹 단위로 1회 롤)")]
            public float chance;

            [Tooltip("이 태그를 '모두' 가진 몬스터만 대상. None이면 모든 몬스터")]
            public MonsterTag requiredTags;

            [Tooltip("이 태그를 하나라도 가지면 제외. None이면 제외 없음")]
            public MonsterTag excludedTags;

            public bool Matches(int difficulty, MonsterTag unitTags)
            {
                if (trait == null) return false;
                if (minDifficulty > difficulty) return false;
                if (requiredTags != MonsterTag.None && (unitTags & requiredTags) != requiredTags) return false;
                if (excludedTags != MonsterTag.None && (unitTags & excludedTags) != MonsterTag.None) return false;
                return true;
            }
        }

        [SerializeField] private List<TraitRule> _rules = new();

        [Tooltip("한 몬스터(그룹)가 가질 수 있는 행동 특성 최대 개수. 0이면 무제한")]
        [SerializeField] private int _maxBehaviorTraits = 2;
        [Tooltip("한 몬스터(그룹)가 가질 수 있는 사망 특성 최대 개수. 0이면 무제한")]
        [SerializeField] private int _maxDeathTraits = 1;

        public IReadOnlyList<TraitRule> Rules => _rules;
        public int MaxBehaviorTraits => _maxBehaviorTraits;
        public int MaxDeathTraits => _maxDeathTraits;
    }
}
