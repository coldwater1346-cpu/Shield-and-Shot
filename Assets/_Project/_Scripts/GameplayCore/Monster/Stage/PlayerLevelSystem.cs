using System;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Progression
{
    /// 스테이지 단위 경험치·레벨. 킬 XP 누적 → 임계치 넘으면 레벨업 이벤트.
    public class PlayerLevelSystem : MonoBehaviour
    {
        public static PlayerLevelSystem Instance { get; private set; }

        [Header("경험치 곡선: baseXp * level^growth")]
        [SerializeField] private float _baseXp = 30f;
        [SerializeField] private float _growth = 2f;

        public int Level { get; private set; } = 1;
        public int CurrentXp { get; private set; }
        public int XpToNext { get; private set; }

        public event Action<int, int> XpChanged;   // (현재, 다음까지)
        public event Action<int> LeveledUp;         // (새 레벨)

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        /// 스테이지 시작 시 호출 (레벨 1부터)
        public void ResetLevel()
        {
            Level = 1;
            CurrentXp = 0;
            XpToNext = RequiredXp(Level);
            XpChanged?.Invoke(CurrentXp, XpToNext);
        }

        public void AddXp(int amount)
        {
            if (amount <= 0) return;
            CurrentXp += amount;

            while (CurrentXp >= XpToNext)   // 한 번에 여러 레벨 가능
            {
                CurrentXp -= XpToNext;
                Level++;
                XpToNext = RequiredXp(Level);
                LeveledUp?.Invoke(Level);
            }
            XpChanged?.Invoke(CurrentXp, XpToNext);
        }

        private int RequiredXp(int level) => Mathf.CeilToInt(_baseXp * Mathf.Pow(level, _growth));
    }
}