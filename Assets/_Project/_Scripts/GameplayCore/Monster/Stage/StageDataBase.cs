using Shield_Shot.GameplayCore.Monster.Difficulty;
using Shield_Shot.UI;
using System.Collections.Generic;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.Stage
{
    public enum GameMode
    {
        Story,
        Infinite
    }

    public class StageDatabase : MonoBehaviour
    {
        public static StageDatabase Instance { get; private set; }

        [Header("스토리: 챕터 목록 (전체 스테이지를 순서대로 선형 진행)")]
        [SerializeField] private List<ChapterSO> _chapters = new();

        [Header("무한 모드 소스")]
        [SerializeField] private EndlessSourceSO _endlessSource;
        private GameMode _mode = GameMode.Story;
        [SerializeField] private int _globalIndex = 0;   // 전체 평탄화 스테이지 인덱스

        private readonly List<(int chapter, int stage)> _flat = new();
        private bool _flatBuilt;
        public GameMode Mode => _mode;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _mode = (GameMode)PlayerPrefs.GetInt("GameMode", (int)GameMode.Story);  // ← 복원
            BuildFlat();
        }

        private void BuildFlat()
        {
            _flat.Clear();
            for (int c = 0; c < _chapters.Count; c++)
            {
                var ch = _chapters[c];
                if (ch == null) continue;
                for (int s = 0; s < ch.StageCount; s++) _flat.Add((c, s));
            }
            _flatBuilt = true;
        }

        private void EnsureFlat() { if (!_flatBuilt) BuildFlat(); }

        // ── 조회 ──────────────────────────────
        public int TotalStageCount { get { EnsureFlat(); return _flat.Count; } }

        public ChapterBiom GetCurrentBiom()
        {
            if (_mode == GameMode.Infinite) return ChapterBiom.None;   // 무한모드는 바이옴 없음
            EnsureFlat();
            if (_flat.Count == 0) return ChapterBiom.None;
            var (c, _) = _flat[Clamped()];
            return _chapters[c] != null ? _chapters[c].Biom : ChapterBiom.None;
        }

        public int GetCurrentStageInChapter()
        {
            EnsureFlat();
            if (_flat.Count == 0) return 0;
            var (_, s) = _flat[Clamped()];
            return s;
        }

        // ── 로비 UI 호출 ──────────────────────
        public void SelectStory(int globalIndex)
        {
            _mode = GameMode.Story;
            PlayerPrefs.SetInt("GameMode", (int)_mode);
            SetGlobalIndex(globalIndex);
        }

        public bool SelectStory(ChapterBiom biom, int biomeStageIndex)
        {
            if (!TryGetLocation(biom, biomeStageIndex, out int c, out int s, out int global))
            {
                Debug.LogWarning($"[StageDB] SelectStory({biom},{biomeStageIndex}) 해석 실패 → 인덱스 안 바뀜(0 유지)");
                return false;
            }
            Debug.Log($"[StageDB] SelectStory({biom},{biomeStageIndex}) → ch {c}, stage {s}, global {global}");
            SelectStory(global);
            return true;
        }
        private int Clamped() => Mathf.Clamp(_globalIndex, 0, Mathf.Max(0, _flat.Count - 1));
        public int CurrentGlobalIndex => _globalIndex;
        public int Count => TotalStageCount;         // 구 API 호환

        public void SelectInfinite()
        {
            _mode = GameMode.Infinite;
            PlayerPrefs.SetInt("GameMode", (int)_mode);
        }


        /// <summary>글로벌 인덱스 → (챕터, 챕터내 스테이지). UI 브라우징용(상태 안 바꿈).</summary>
        public bool TryGetLocation(int globalIndex, out int chapterIndex, out int stageInChapter)
        {
            EnsureFlat();
            if (globalIndex < 0 || globalIndex >= _flat.Count) { chapterIndex = 0; stageInChapter = 0; return false; }
            var (c, s) = _flat[globalIndex];
            chapterIndex = c; stageInChapter = s; 
            return true;
        }

        public bool TryGetLocation(ChapterBiom biom, int biomeStageIndex, out int chapterIndex, out int stageInChapter, out int globalIndex)
        {
            EnsureFlat();
            chapterIndex = 0; stageInChapter = 0; globalIndex = 0;
            if (biomeStageIndex < 0) return false;

            int count = 0;
            for (int g = 0; g < _flat.Count; g++)
            {
                var (c, s) = _flat[g];
                var ch = _chapters[c];
                if (ch == null || ch.Biom != biom) continue;   // 바이옴 필터

                if (count == biomeStageIndex)
                {
                    chapterIndex = c; stageInChapter = s; globalIndex = g;
                    return true;
                }
                count++;
            }
            return false;   // 그 바이옴에 해당 인덱스 스테이지 없음
        }

        public int CurrentStage => _globalIndex;     // 구 API 호환

        public int GetCurrentStage() => CurrentGlobalIndex + 1;

        public string GetChapterName(int chapterIndex)
            => (chapterIndex >= 0 && chapterIndex < _chapters.Count && _chapters[chapterIndex] != null)
                ? _chapters[chapterIndex].DisplayName : "";
        public void SetStage(int index) => SelectStory(index);   // 구 API 호환

        public void SetGlobalIndex(int index)
        {
            EnsureFlat();
            if (_flat.Count == 0) { _globalIndex = 0; return; }
            _globalIndex = Mathf.Clamp(index, 0, _flat.Count - 1);

            Debug.Log($"[StageDB] SetGlobalIndex 요청 {index} → 적용 {_globalIndex} (총 {_flat.Count})");
        }

        // ── StageManager 호출: 선택된 스테이지 하나만 반환 ──
        public IDifficultyWaveSource GetWaveSource()
        {
            if (_mode == GameMode.Infinite) return _endlessSource;

            EnsureFlat();
            if (_flat.Count == 0) return null;
            var (c, s) = _flat[Clamped()];
            return _chapters[c].CreateStageSource(s);
        }

        // 서버 연동 로직
        //public void SetRuntimeData(StoryStageSO storyData)
        //{
        //    // 기존 리스트를 서버 데이터로 교체하거나 업데이트
        //    if (storyData != null) _stages = new List<StoryStageSO> { storyData };
        //}

    }
}