using System.Collections;
using Shield_Shot.DataManagement;
using Shield_Shot.GameplayCore.Monster.Core;
using Shield_Shot.GameplayCore.Monster.Difficulty;
using Shield_Shot.UI;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.Stage
{
    /// <summary>
    /// 스테이지 조율자(Facade). 직접 로직은 갖지 않고 하위 시스템에 위임한다.
    ///  - 웨이브 소스 결정 → 아레나 초기화 → WaveRunner에 진행 위임 → 클리어 시 결과 처리.
    /// </summary>
    public class StageManager : MonoBehaviour
    {
        public static StageManager Instance { get; private set; }

        [Header("웨이브 소스 (Chapter / StageDefinition / Endless 중 택1)")]
        [Tooltip("IDifficultyWaveSource 를 구현한 SO. StageDatabase 가 있으면 그쪽이 우선.")]
        [SerializeField] private ScriptableObject _waveSource;
        [SerializeField] private WaveRunner _waveRunner;

        [Header("Arena Scene Init")]
        [SerializeField] private bool _initializeArenaOnStart = true;
        [SerializeField] private ArenaInitializer _arena;

        [Header("종료 처리")]
        [SerializeField] private StageResultHandler _resultHandler;

        [Header("생존 추적 (분열체 등록용)")]
        [SerializeField] private AliveTracker _aliveTracker;

        [Header("테스트 모드 (StageDatabase 없이 강제)")]
        [SerializeField] private bool _testMode = false;
        [SerializeField] private ChapterBiom _testBiom;
        [SerializeField] private int _testStageIndex = 0;
        [SerializeField] private int _testGlobalIndex = 0;

        [Header("디버그 (읽기 전용)")]
        [SerializeField] private int _debugStageNumber;

        [SerializeField] private int _rewardGold;
        private IDifficultyWaveSource _source;

        public int RewardGold => _rewardGold;
        public int KillCount { get; private set; }
        public ChapterBiom CurrentBiom { get; private set; }
        public int CurrentStageIndex { get; private set; }
        public int CurrentWave { get; private set; } = 1;

        // ─── 생명주기 ────────────────────────────────────
        private void Awake()
        {
            if (_waveRunner != null) _waveRunner.StageCleared += OnStageClear;
            if (_waveRunner != null) _waveRunner.WaveCleared += OnWaveClear;
        }

        private void OnDestroy()
        {
            if (_waveRunner != null) _waveRunner.StageCleared -= OnStageClear;
            if (_waveRunner != null) _waveRunner.WaveCleared -= OnWaveClear;
        }

        private void Start()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }

            StartCoroutine(StartStageFlow());
        }

        private IEnumerator StartStageFlow()
        {
            yield return null;
            ResolveWaveSource();

            if (_initializeArenaOnStart)
            {
                yield return null;
                _arena?.Initialize();
                yield return null;
                _arena?.ApplyCameraPreset();
            }

            StartStage();
        }

        // ─── 소스 결정 ───────────────────────────────────
        // StageDatabase 가 소스를 제공하면 그걸, 없으면(또는 테스트 모드면) 인스펙터 SO 사용.
        private void ResolveWaveSource()
        {
            _source = _waveSource as IDifficultyWaveSource;
            if (_testMode) return;   // 테스트: 인스펙터 SO 그대로

            var db = StageDatabase.Instance;
            if (db != null && db.GetWaveSource() is IDifficultyWaveSource fromDb && fromDb != null)
                _source = fromDb;

            if (_source is EndlessSourceSO endless)
            {
                int score = GetPlayerEquipmentScore();
                if (score > 0) endless.StartDifficultyOverride = score;
            }
        }

        // ─── 스테이지 시작 ───────────────────────────────
        public void StartStage()
        {
            KillCount = 0;
            Progression.PlayerLevelSystem.Instance?.ResetLevel();
            if (_source == null) { Debug.LogError("[Stage] 웨이브 소스 미설정"); return; }

            var db = StageDatabase.Instance;
            if (_testMode)
            {
                CurrentBiom = _testBiom;
                CurrentStageIndex = _testStageIndex;
            }
            else if (db != null)
            {
                CurrentBiom = db.GetCurrentBiom();
                CurrentStageIndex = db.GetCurrentStageInChapter();
            }

            _arena?.ValidateTheme();
            UIEventBus.RaiseStageChanged(CurrentStageIndex + 1);

            _rewardGold = (_source is IStageReward reward) ? reward.RewardGold : 0;   // ← SO에서

            _waveRunner.Begin(_source);
        }

        private int GetPlayerEquipmentScore()
        {
            // TODO: 실제 장비 점수 연동. 0이면 EndlessSourceSO 의 기본 시작 난이도 사용
            return 0;
        }

        // ─── 종료 위임 ───────────────────────────────────
        private void OnStageClear()
        {
            _debugStageNumber = StageDatabase.Instance != null ? StageDatabase.Instance.GetCurrentStage() : -1;
            _resultHandler.ShowVictory(_rewardGold, CurrentBiom, CurrentStageIndex);
        }

        public void StageFall()
        {
            if (StageDatabase.Instance.Mode == GameMode.Infinite)
            {
                InfinityModeReward();
            }

            _resultHandler.ShowDefeat();
        }

        // 분열체 등록 (SplitNode → AliveTracker 위임)
        public void RegisterMonster(MonsterBase m) => _aliveTracker.Register(m);

        public void AddKillCount()
        {
            KillCount++;
        }

        private void InfinityModeReward()
        {
            _rewardGold = KillCount * 10;
        }

        private void OnWaveClear()
        {
            CurrentWave = _waveRunner.CurrentWaveIndex;
            UIEventBus.RaiseWaveChanged(CurrentWave);
        }
    }
}