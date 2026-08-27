using Shield_Shot.GameplayCore.Monster.Core;
using Shield_Shot.UI;
using UnityEngine;
// UIEventBus 를 해석하는 using을 StageManager에서 그대로 가져오세요.

namespace Shield_Shot.GameplayCore.Monster.Stage
{
    /// 보스 스폰 시 체력바 UI 연동.
    public class BossPresenter : MonoBehaviour
    {
        [SerializeField] private BossHealthUI _bossHealthUI;
        [SerializeField] private MonsterSpawner _spawner;

        private void OnEnable() { if (_spawner != null) _spawner.BossSpawned += OnBossSpawned; }
        private void OnDisable() { if (_spawner != null) _spawner.BossSpawned -= OnBossSpawned; }

        private void Awake()
        {
            if (_bossHealthUI == null)
                _bossHealthUI = FindFirstObjectByType<BossHealthUI>(FindObjectsInactive.Include);
        }

        public void OnBossSpawned(MonsterBase boss)
        {
            if (_bossHealthUI != null) _bossHealthUI.SetMaxHealth(boss.Health.MaxHealth);
            UIEventBus.RaiseBossHpBar(true);
        }
    }
}