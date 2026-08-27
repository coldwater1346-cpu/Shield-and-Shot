#region USING
using Shield_Shot.GameplayCore.Field;
using Shield_Shot.GameplayCore.Monster.Attack;
using Shield_Shot.GameplayCore.Monster.BT;
using Shield_Shot.GameplayCore.Monster.BT.Actions;
using Shield_Shot.GameplayCore.Monster.BT.Conditions;
using Shield_Shot.GameplayCore.Monster.BT.Core;
using Shield_Shot.GameplayCore.Monster.BT.Nodes;
using Shield_Shot.GameplayCore.Monster.BT.Traits;
using Shield_Shot.GameplayCore.Monster.Difficulty;
using Shield_Shot.GameplayCore.Monster.Movement;
using Shield_Shot.GameplayCore.Monster.Stage;
using Shield_Shot.GameplayCore.Monster.Status;
using Shield_Shot.GameplayCore.Monster.Wave;
using Shield_Shot.GameplayCore.Render;
using System;
using System.Collections.Generic;
using UnityEngine;

#endregion

namespace Shield_Shot.GameplayCore.Monster.Core
{
    #region REQUIRES
    [RequireComponent(typeof(HealthComponent))]
    [RequireComponent(typeof(MovementComponent))]
    [RequireComponent(typeof(BtRunner))]
    [RequireComponent(typeof(AnimComponent))]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(DeadLineDetector))]
    [RequireComponent(typeof(LookAtPlayer))]
    [RequireComponent(typeof(PlayerCharger))]
    [RequireComponent(typeof(StatusEffectController))]
    [RequireComponent(typeof(ElementFieldEffectTarget))]
    #endregion

    public class MonsterBase : MonoBehaviour
    {
        [SerializeField] private HealthComponent _healthComponent;
        [SerializeField] private MovementComponent _movementComponent;
        [SerializeField] private AttackComponent _attackComponent;
        [SerializeField] private VFXType _deathVFXType = VFXType.MonsterDeath;

        private readonly List<IMovementBehavior> _activeBehaviors = new();
        private readonly List<BtTraitSO> _behaviorTraitBuf = new();
        private readonly List<BtTraitSO> _deathTraitBuf = new();
        private BtRunner _btRunner;
        private int _xpReward;

        public Action<MonsterBase> OnReturnToPool;
        public GameObject SourcePrefab { get; set; }
        public MonsterAttackPoints AttackPoints { get; private set; }
        public float AttackDamage { get; private set; }

        public HealthComponent Health => _healthComponent;
        public MovementComponent Movement => _movementComponent;
        public AttackComponent Attack => _attackComponent;
        public IReadOnlyList<IMovementBehavior> ActiveBehaviors => _activeBehaviors;

        private void Awake()
        {
            if (!_healthComponent) _healthComponent = GetComponent<HealthComponent>();
            if (!_movementComponent) _movementComponent = GetComponent<MovementComponent>();
            if (!_attackComponent) _attackComponent = GetComponent<AttackComponent>();
            if (!AttackPoints) AttackPoints = GetComponent<MonsterAttackPoints>();
            _btRunner = GetComponent<BtRunner>();
        }

        private void OnEnable()
        {            
            _healthComponent.OnDeath += GrantXpOnDeath;
            _healthComponent.OnDeath += AddStageManagerKillCount;

        }
        private void OnDisable()
        {
            _healthComponent.OnDeath -= GrantXpOnDeath;
            _healthComponent.OnDeath -= AddStageManagerKillCount;

        }

        public void SetColliderEnabled(bool enabled)
        {
            if (TryGetComponent<CapsuleCollider>(out var col))
                col.enabled = enabled;
        }


        #region 난이도 기반 로직 (그룹 주입)
        // 대형(그룹)에서 이미 확정된 종류·스탯·이동·특성을 그대로 주입받는다.
        // 개체는 재추첨하지 않으므로 같은 대형이면 자동으로 동종·동특성.
        public void Initialize(MonsterGroupPlan group, int difficulty)
        {
            var unit = group.Unit;

            _movementComponent.enabled = true;
            if (TryGetComponent<CapsuleCollider>(out var col)) col.enabled = true;
            GetComponent<AnimComponent>()?.ResetAnim();

            // 그룹 공유 값 복사 (다시 굴리지 않음)
            _activeBehaviors.Clear(); _activeBehaviors.AddRange(group.Movements);
            _behaviorTraitBuf.Clear(); _behaviorTraitBuf.AddRange(group.BehaviorTraits);
            _deathTraitBuf.Clear(); _deathTraitBuf.AddRange(group.DeathTraits);

            AttackDamage = group.Stats.AttackDamage;                       // 공격력도 스케일 반영
            _healthComponent.Initialize(group.Stats.MaxHP);                // HP
            _movementComponent.Initialize(group.Stats.MoveSpeed, _activeBehaviors); // 이동속도

            if (unit.IsBoss)
            {
                _deathTraitBuf.Clear();
                _attackComponent?.SetPatterns(unit.AttackPatterns);
                _btRunner.Initialize(this, MonsterBtFactory.BuildBoss(_deathTraitBuf, unit.FireInterval, _deathVFXType));
            }
            else
            {
                _btRunner.Initialize(this, MonsterBtFactory.BuildNormal(_behaviorTraitBuf, _deathTraitBuf, _deathVFXType));
            }
            _xpReward = unit.XpReward;
        }
        #endregion

        // 분열 등 단순 초기화
        // Before: public void InitializeSimple(MonsterDataSO data, List<IMovementBehavior> behaviors)
        public void InitializeSimple(float maxHp, float moveSpeed, List<IMovementBehavior> behaviors)
        {
            _movementComponent.enabled = true;
            if (TryGetComponent<CapsuleCollider>(out var col)) col.enabled = true;
            GetComponent<AnimComponent>()?.ResetAnim();
            _activeBehaviors.Clear();
            if (behaviors != null) _activeBehaviors.AddRange(behaviors);
            _healthComponent.Initialize(maxHp);              // data.MaxHP → maxHp
            _movementComponent.Initialize(moveSpeed, _activeBehaviors);  // data.MoveSpeed → moveSpeed
            _btRunner.Initialize(this, MonsterBtFactory.BuildSimple(_deathVFXType));
        }

        private void GrantXpOnDeath() =>
            Progression.PlayerLevelSystem.Instance?.AddXp(_xpReward);

        private void AddStageManagerKillCount()
        {
            StageManager.Instance?.AddKillCount();
        }

    }
}