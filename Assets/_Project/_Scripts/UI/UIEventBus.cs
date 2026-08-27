using System;

namespace Shield_Shot.UI
{
    public static class UIEventBus
    {
        public static Action<MenuType> OnMenuChanged;
        public static Action OnEquipmentChanged;
        public static Action OnInventoryUpdated;
        public static Action OnCurrencyChanged;
        public static Action<int> OnStageSelected;

        public static Action<int> OnStageChanged;
        public static Action<int> OnWaveChanged;
        public static Action<int> OnGoldChanged;
        public static Action<int> OnMonsterCountChanged;
        public static Action<float> OnPlayerHealthChanged;
        public static Action<float> OnBossHealthChanged;
        public static Action<bool> OnBossHpBarChanged;
        
        public static void RaiseMenuChanged(MenuType menuType) => OnMenuChanged?.Invoke(menuType);
        public static void RaiseEquipmentChanged() => OnEquipmentChanged?.Invoke();
        public static void RaiseInventoryUpdated() => OnInventoryUpdated?.Invoke();
        public static void RaiseCurrencyChanged() => OnCurrencyChanged?.Invoke();
        public static void RaiseStageSelected(int stageNum) => OnStageSelected?.Invoke(stageNum);

        public static void RaiseStageChanged(int stageNum) => OnStageChanged?.Invoke(stageNum);
        public static void RaiseWaveChanged(int currentWave) =>OnWaveChanged?.Invoke(currentWave);
        public static void RaiseGoldChanged(int gold) => OnGoldChanged?.Invoke(gold);
        public static void RaiseMonsterCountChanged(int monsterCount) => OnMonsterCountChanged?.Invoke(monsterCount);
        public static void RaisePlayerHealthChanged(float hp) => OnPlayerHealthChanged?.Invoke(hp);
        public static void RaiseBossHealthChanged(float hp) => OnBossHealthChanged?.Invoke(hp);
        public static void RaiseBossHpBar(bool isBoss) => OnBossHpBarChanged?.Invoke(isBoss);
    }
}

