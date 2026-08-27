using Shield_Shot.GameplayCore.Monster.BT.Traits;
using Shield_Shot.GameplayCore.Monster.Core;
using Shield_Shot.GameplayCore.Monster.Movement;
using Shield_Shot.GameplayCore.Monster.Spawn;
using Shield_Shot.GameplayCore.Monster.Wave;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.Wave
{
    // ── 확률형 이동 행동 래퍼 ──────────────────────────────
    [Serializable]
    public class OptionalMovementBehavior
    {
        public ScriptableObject behavior;
        [Range(0f, 1f)] public float probability = 1f;

        // 생성자 추가
        public OptionalMovementBehavior(ScriptableObject behavior, float probability)
        {
            this.behavior = behavior;
            this.probability = probability;
        }
    }
}

    // ── 수량별 포메이션 규칙 ──────────────────────────────
    [Serializable]
    public class FormationRule
    {
        [Tooltip("이 수 이상일 때 이 포메이션 사용")]
        public int minCount;
        public ScriptableObject formation;
        [Min(0f)] public float weight = 1f;

    // 생성자 추가
    public FormationRule(int minCount, ScriptableObject formation, float weight)
    {
        this.minCount = minCount;
        this.formation = formation;
        this.weight = weight;
    }
}

    [Serializable]
    public class BossAttackRule
    {
        public ScriptableObject attackPattern;
    }

    // ── 스폰 그룹 ─────────────────────────────────────────
    [Serializable]
    public struct SpawnGroup
    {
        [Header("Monster")]
        [SerializeField] private GameObject monsterPrefab;
        [SerializeField] private MonsterDataSO monsterData;

        [Header("Movement")]
        [SerializeField] private List<OptionalMovementBehavior> movementBehaviors;

        [Header("BT Traits")]
        [Header("생존 중 행동 특성")]
        [SerializeField] private List<BtTraitSO> _behaviorTraits; // 생존 중 행동 (회피 등)
        [Header("사망 처리 특성 ")]
        [SerializeField] private List<BtTraitSO> _deathTraits;    // 사망 처리 (부활, 분열 등)

        [Header("Selection Weight")]
        [SerializeField] private float weight;

        public GameObject MonsterPrefab => monsterPrefab;
        public MonsterDataSO MonsterData => monsterData;
        public float Weight => Mathf.Max(0f, weight);

        public IReadOnlyList<BtTraitSO> BehaviorTraits => _behaviorTraits ?? new List<BtTraitSO>();
        public IReadOnlyList<BtTraitSO> DeathTraits => _deathTraits ?? new List<BtTraitSO>();

        public void GetBehaviors(List<IMovementBehavior> outList)
        {
            outList.Clear();
            if (movementBehaviors == null) return;
            foreach (var opt in movementBehaviors)
                if (opt.behavior is IMovementBehavior b && UnityEngine.Random.value <= opt.probability)
                    outList.Add(b);
        }
    /// <summary>
    /// 뒤끝 파싱 매니저에서 데이터들을 주입하는 초기화 함수
    /// </summary>
    public void Initialize(GameObject prefab, MonsterDataSO data, float weight,
                           List<OptionalMovementBehavior> movements,
                           List<BtTraitSO> behaviorTraits, List<BtTraitSO> deathTraits)
    {
        this.monsterPrefab = prefab;
        this.monsterData = data;
        this.weight = weight;
        this.movementBehaviors = movements ?? new List<OptionalMovementBehavior>();
        this._behaviorTraits = behaviorTraits ?? new List<BtTraitSO>();
        this._deathTraits = deathTraits ?? new List<BtTraitSO>();
    }
}

    // ── 보스 그룹 ─────────────────────────────────────────
    [Serializable]
    public struct BossGroup
    {
        [Header("Monster")]
        [SerializeField] private GameObject monsterPrefab;
        [SerializeField] private MonsterDataSO monsterData;

        [Header("Attack Patterns")]
        [SerializeField] private List<BossAttackRule> attackPatterns;
        [SerializeField] private float _fireInterval;

        [Header("BT Traits")]
        [SerializeField] private List<BtTraitSO> _deathTraits;

        public GameObject MonsterPrefab => monsterPrefab;
        public MonsterDataSO MonsterData => monsterData;
        public List<BossAttackRule> AttackPatterns => attackPatterns;
        public float FireInterval => _fireInterval;
        public IReadOnlyList<BtTraitSO> DeathTraits => _deathTraits ?? new List<BtTraitSO>();

    /// <summary>
    /// 뒤끝 파싱 매니저에서 보스 데이터들을 주입해주는 초기화 함수
    /// </summary>
    public void Initialize(GameObject prefab, MonsterDataSO data, List<BossAttackRule> patterns, float fireInterval, List<BtTraitSO> deathTraits)
    {
        this.monsterPrefab = prefab;
        this.monsterData = data;
        this.attackPatterns = patterns ?? new List<BossAttackRule>();
        this._fireInterval = fireInterval;
        this._deathTraits = deathTraits ?? new List<BtTraitSO>();
    }
}

    // ── WaveSO ────────────────────────────────────────────
    [CreateAssetMenu(fileName = "WaveSO", menuName = "Monster/Wave")]
    public class WaveSO : ScriptableObject
    {
        [Header("웨이브 설정")]
        [SerializeField] private float _spawnInterval = 3f;
        [SerializeField] private int _totalSpawnCount = 20;
        [SerializeField] private int _minPerSpawn = 1;
        [SerializeField] private int _maxPerSpawn = 5;

        [Header("Boss Wave")]
        [SerializeField] private bool _isBossWave;
        [SerializeField] private BossGroup _bossGroup;

        public bool IsBossWave => _isBossWave;
        public BossGroup BossGroup => _bossGroup;

        [Header("수량별 포메이션")]
        [SerializeField] private List<FormationRule> _formationRules;

    // WaveSO  추가 프로퍼티
    public List<FormationRule> FormationRulesList => _formationRules;

    [Header("스폰 그룹 (가중치 랜덤)")]
        [SerializeField] private List<SpawnGroup> _spawnGroups;

        public float SpawnInterval => _spawnInterval;
        public int TotalSpawnCount => _totalSpawnCount;
        public IReadOnlyList<SpawnGroup> SpawnGroups => _spawnGroups;

        public int RollSpawnCount(int remaining)
        {
            int roll = UnityEngine.Random.Range(_minPerSpawn, _maxPerSpawn + 1);
            return Mathf.Min(roll, remaining);
        }

        public ISpawnFormation GetFormationForCount(int count)
        {
            if (_formationRules == null) return null;

            int bestMin = -1;
            foreach (var rule in _formationRules)
                if (rule.minCount <= count && rule.minCount > bestMin)
                    bestMin = rule.minCount;

            if (bestMin < 0) return null;

            float total = 0f;
            foreach (var rule in _formationRules)
                if (rule.minCount == bestMin) total += rule.weight;

            float rand = UnityEngine.Random.Range(0f, total);
            float cum = 0f;
            foreach (var rule in _formationRules)
            {
                if (rule.minCount != bestMin) continue;
                cum += rule.weight;
                if (rand <= cum) return rule.formation as ISpawnFormation;
            }

            return null;
        }

        public bool TryPickSpawnGroup(out SpawnGroup result)
        {
            result = default;
            if (_spawnGroups == null || _spawnGroups.Count == 0) return false;

            float total = 0f;
            foreach (var g in _spawnGroups) total += g.Weight;

            if (total <= 0f) { result = _spawnGroups[0]; return true; }

            float rand = UnityEngine.Random.Range(0f, total);
            float cum = 0f;
            foreach (var g in _spawnGroups)
            {
                cum += g.Weight;
                if (rand <= cum) { result = g; return true; }
            }

            result = _spawnGroups[_spawnGroups.Count - 1];
            return true;
        }

    /// <summary> 
    /// 웨이브의 기본 정보 세팅
    /// </summary>
    public void InitializeMaster(float spawnInterval, int totalSpawnCount, int minPerSpawn, int maxPerSpawn, bool isBossWave)
    {
        _spawnInterval = spawnInterval;
        _totalSpawnCount = totalSpawnCount;
        _minPerSpawn = minPerSpawn;
        _maxPerSpawn = maxPerSpawn;
        _isBossWave = isBossWave;

       
        if (_spawnGroups == null)
            _spawnGroups = new List<SpawnGroup>();

        if (_formationRules == null)
            _formationRules = new List<FormationRule>();
    }
 
    //  SpawnGroup 리스트 주입

    public void SetSpawnGroups(List<SpawnGroup> groups)
    {
        _spawnGroups = groups;
    }

    //FormationRule 리스트 주입
    public void SetFormationRules(List<FormationRule> rules)
    {
        _formationRules = rules;
    }

    //  BossGroup 구조체 주입
    public void SetBossGroup(BossGroup bossGroup)
    {
        _bossGroup = bossGroup;
    }
}
