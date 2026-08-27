using Shield_Shot.GameplayCore.Augment;
using Shield_Shot.GameplayCore.Monster.Difficulty;
using Shield_Shot.GameplayCore.Monster.Pool;
using Shield_Shot.UI;
using System;
using System.Collections;
using UnityEngine;
// UIEventBus, AugmentPopupUI 를 해석하는 using을 StageManager에서 그대로 가져오세요.

namespace Shield_Shot.GameplayCore.Monster.Stage
{
    /// 웨이브 루프: 구성 → 스폰 → 전멸 대기 → 증강 게이트 → 다음. 소진 시 StageCleared.
    public class WaveRunner : MonoBehaviour
    {
        [SerializeField] private MonsterSpawner _spawner;
        [SerializeField] private AliveTracker _aliveTracker;
        //[SerializeField] private AugmentPopupUI _augmentPopup;   // 없으면 팝업 없이 진행

        [SerializeField] private float _batchInterval = 1.5f;
        [SerializeField] private float _nextWaveDelay = 3f;
        [SerializeField] private int _prewarmPerPrefab = 12;

        [SerializeField] private int _debugWaveNumber;   // 읽기전용

        public event Action StageCleared;
        public event Action WaveCleared;

        private IDifficultyWaveSource _source;
        private readonly DifficultyWaveComposer _composer = new();

        private int _currentWaveIndex;
        private bool _spawningDone;

        public int CurrentWaveIndex => _currentWaveIndex + 1;

        private MonsterUnitPoolSO.UnitGroup _activeGroup;
        private MonsterUnitPoolSO _activePool;
        private int _activeStageKey = -1;

        private void OnEnable()
        {
            if (_spawner != null) _spawner.Spawned += _aliveTracker.Register;
            if (_aliveTracker != null) _aliveTracker.CountChanged += OnAliveCountChanged;
        }

        private void OnDisable()
        {
            if (_spawner != null) _spawner.Spawned -= _aliveTracker.Register;
            if (_aliveTracker != null) _aliveTracker.CountChanged -= OnAliveCountChanged;
        }

        public void Begin(IDifficultyWaveSource source)
        {
            ResetRunner();   // ← 이전 실행 잔재 정리 (재시작 안전)

            _source = source;
            foreach (var pool in _source.EnumeratePools())
                MonsterFactory.Instance.Prewarm(pool, _prewarmPerPrefab);

            _currentWaveIndex = 0;
            _activeGroup = null; _activePool = null; _activeStageKey = -1;
            StartWave();
        }

        private void ResetRunner()
        {
            StopAllCoroutines();   // WaveRoutine / NextWaveDelay 전부 중단
            //if (_augmentPopup != null)
            //    _augmentPopup.OnAugmentSelectionCompleted -= HandleAugmentDone;   // 중복/잔여 구독 제거
            _spawningDone = false;
        }


        private void StartWave()
        {
            if (_source == null || !_source.TryGetWave(_currentWaveIndex, out ResolvedWave ctx))
            {
                _debugWaveNumber = _currentWaveIndex + 1;
                StageCleared?.Invoke();
                return;
            }

            // 스테이지가 바뀌면 테마 그룹 재선정 (한 스테이지 = 한 그룹)
            if (ctx.pool != null &&
                (ctx.pool != _activePool || ctx.stageIndex != _activeStageKey || _activeGroup == null))
            {
                _activeGroup = ctx.pool.PickGroup();
                _activePool = ctx.pool;
                _activeStageKey = ctx.stageIndex;
            }
            ctx.group = _activeGroup;

            _spawningDone = false;
            _aliveTracker.Reset();

            DifficultyWavePlan plan = _composer.Compose(ctx);
            StartCoroutine(WaveRoutine(plan));
        }

        private IEnumerator WaveRoutine(DifficultyWavePlan plan)
        {
            if (plan.IsBoss && plan.BossGroup != null)
                _spawner.SpawnBossGroup(plan.BossGroup, plan.Difficulty);

            foreach (var group in plan.Groups)
            {
                _spawner.SpawnGroup(group, plan.Difficulty);
                yield return new WaitForSeconds(_batchInterval);
            }

            _spawningDone = true;
            CheckWaveClear();
        }

        private void OnAliveCountChanged(int count)
        {
            UIEventBus.RaiseMonsterCountChanged(count);   // HUD
            CheckWaveClear();
        }

        private void CheckWaveClear()
        {
            if (!_spawningDone || _aliveTracker.AliveCount > 0) return;

            _spawningDone = false;   // 재진입 방지
            _currentWaveIndex++;
            WaveCleared?.Invoke();
            StartCoroutine(NextWaveDelay());
        }

        //private void HandleAugmentDone()
        //{
        //    _augmentPopup.OnAugmentSelectionCompleted -= HandleAugmentDone;
        //    StartCoroutine(NextWaveDelay());
        //}

        private IEnumerator NextWaveDelay()
        {
            yield return new WaitForSeconds(_nextWaveDelay);
            StartWave();
        }
    }
}