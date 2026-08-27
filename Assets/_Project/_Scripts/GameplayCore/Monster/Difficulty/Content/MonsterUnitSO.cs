using System.Collections.Generic;
using UnityEngine;
using Shield_Shot.GameplayCore.Monster.BT.Traits;
using Shield_Shot.GameplayCore.Monster.Movement;
using Shield_Shot.GameplayCore.Monster.Wave;   // OptionalMovementBehavior, BossAttackRule

namespace Shield_Shot.GameplayCore.Monster.Difficulty
{
    /// <summary>
    /// 한 종류의 몬스터/보스 "설계도". 기존 MonsterEntrySO + BossEntrySO + MonsterDataSO 의 통합 후계.
    ///
    /// 여기에는 난이도로 안 변하는 "정체성" 만 담는다:
    ///   - 프리팹 / 기본 스탯(원본) / 성장 성향 / 태그(특성 매칭용)
    ///   - 등장 조건 / 예산 비용 / 선택 가중치 / 확률형 이동
    ///
    /// [바뀐 점] 특성 리스트를 여기 다 채울 필요가 없다.
    ///   특성은 챕터/스테이지의 <see cref="TraitRuleSetSO"/> 에서 정의되고
    ///   난이도에 따라 <see cref="TraitInjector"/> 가 주입한다.
    ///   이 몬스터만의 '고정 시그니처 특성' 이 꼭 필요할 때만 innate 목록을 쓴다(선택).
    /// </summary>
    [CreateAssetMenu(fileName = "MonsterUnit", menuName = "Monster/Difficulty/Monster Unit")]
    public class MonsterUnitSO : ScriptableObject
    {
        public enum Role { Normal, Boss }

        [Header("정체성")]
        [SerializeField] private Role _role = Role.Normal;
        [SerializeField] private GameObject _prefab;
        [Tooltip("특성 규칙이 이 몬스터를 대상으로 삼을지 거르는 태그")]
        [SerializeField] private MonsterTag _tags = MonsterTag.Melee;

        [Header("기본 스탯 (난이도 1 기준 원본)")]
        [SerializeField] private SerializableStatBlock _baseStats;

        [Header("성장 성향 (선택). 비우면 프로파일 수식 그대로")]
        [SerializeField] private MonsterStatProfileSO _statProfile;

        [Header("등장/선택 규칙")]
        [Tooltip("예산에서 차감되는 비용")]
        [SerializeField] private int _cost = 10;
        [Tooltip("이 난이도 이상부터 등장")]
        [SerializeField] private int _minDifficulty = 0;
        [Tooltip("같은 후보군 내 선택 가중치")]
        [SerializeField] private float _weight = 1f;

        [Header("이동 (확률형)")]
        [SerializeField] private List<OptionalMovementBehavior> _movements = new();

        [Header("고정 특성 (선택). 챕터 규칙과 별개로 이 몬스터가 항상 갖는 특성")]
        [SerializeField] private List<BtTraitSO> _innateBehaviorTraits = new();
        [SerializeField] private List<BtTraitSO> _innateDeathTraits = new();

        [Header("보스 전용 (Role=Boss 일 때만)")]
        [SerializeField] private List<BossAttackRule> _attackPatterns = new();
        [SerializeField] private float _fireInterval = 2f;
        [SerializeField] private float _spawnYOffset = 0f;

        [Header("보상")]
        [SerializeField] private int _xpReward = 10;


        // ── 조회 프로퍼티 ──────────────────────────────
        public Role UnitRole => _role;
        public bool IsBoss => _role == Role.Boss;
        public GameObject Prefab => _prefab;
        public MonsterTag Tags => _tags;
        public StatBlock BaseStats => _baseStats.ToRuntime();
        public MonsterStatProfileSO StatProfile => _statProfile;
        public int Cost => Mathf.Max(1, _cost);
        public int MinDifficulty => _minDifficulty;
        public float Weight => Mathf.Max(0f, _weight);

        public List<BossAttackRule> AttackPatterns => _attackPatterns;
        public float FireInterval => _fireInterval;
        public float SpawnYOffset => _spawnYOffset;
        public int XpReward => _xpReward;

        /// <summary>일반 몬스터 후보 조건 (해금 + 예산 내).</summary>
        public bool IsAvailable(int difficulty, int budget)
            => _prefab != null && _role == Role.Normal
               && _minDifficulty <= difficulty && Cost <= budget;

        /// <summary>보스 후보 조건.</summary>
        public bool IsBossAvailable(int difficulty)
            => _prefab != null && _role == Role.Boss && _minDifficulty <= difficulty;

        /// <summary>확률형 이동 행동을 굴려 outList 를 채운다(그룹당 1회 호출).</summary>
        public void GetMovements(List<IMovementBehavior> outList)
        {
            outList.Clear();
            if (_movements == null) return;
            foreach (var opt in _movements)
                if (opt.behavior is IMovementBehavior b && Random.value <= opt.probability)
                    outList.Add(b);
        }

        // ── 고정 특성 수집 (append; TraitInjector 가 버퍼를 먼저 비운 뒤 호출) ──
        public void CollectInnateBehaviorTraits(List<BtTraitSO> outList)
            => Append(_innateBehaviorTraits, outList);

        public void CollectInnateDeathTraits(List<BtTraitSO> outList)
            => Append(_innateDeathTraits, outList);

        private static void Append(List<BtTraitSO> src, List<BtTraitSO> outList)
        {
            if (src == null) return;
            foreach (var t in src)
                if (t != null && !outList.Contains(t)) outList.Add(t);
        }
    }
}
