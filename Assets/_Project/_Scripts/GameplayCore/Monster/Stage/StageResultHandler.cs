using Shield_Shot.Audio;
using Shield_Shot.DataManagement;
using Shield_Shot.NetworkCore;
using Shield_Shot.GameplayCore.Monster.Difficulty;   // ChapterBiom
using Shield_Shot.UI;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.Stage
{
    /// 스테이지 종료 처리: 결과 UI · 보상 지급 · 진행 기록 · 사운드.
    public class StageResultHandler : MonoBehaviour
    {
        [SerializeField] private ResultPanelUI _resultPanel;

        [Header("Sound")]
        [SerializeField] private AudioClip _victorySfx;
        [SerializeField, Range(0f, 1f)] private float _victorySfxVolume = 0.5f;
        [SerializeField] private AudioClip _defeatSfx;
        [SerializeField, Range(0f, 1f)] private float _defeatSfxVolume = 0.5f;

        public void ShowVictory(int rewardGold, ChapterBiom biom, int stageIndex)
        {
            _resultPanel?.ShowResult(true);
            UnlockStage((int)biom, stageIndex);

            if (PlayerDataManager.Instance != null) PlayerDataManager.Instance.gold += rewardGold;
            BackendGameData.Instance?.GameDataUpdateAsync();
            if (SoundManager.Instance != null) SoundManager.Instance.PlayUI(_victorySfx, _victorySfxVolume);
        }

        public void ShowDefeat()
        {
            _resultPanel?.ShowResult(false);

            if (SoundManager.Instance != null) SoundManager.Instance.PlayUI(_defeatSfx, _defeatSfxVolume);
            if(StageDatabase.Instance.Mode == GameMode.Infinite)
            {
                if (PlayerDataManager.Instance != null) PlayerDataManager.Instance.gold += StageManager.Instance.RewardGold;
                BackendGameData.Instance?.GameDataUpdateAsync();
            }
        }

        private void UnlockStage(int chapter, int stage)
        {
            var pdm = PlayerDataManager.Instance;
            if (pdm == null) return;

            int clearedStageId = ((chapter - 1) * 30) + stage;
            if (clearedStageId >= pdm.clearStageStep)
            {
                Debug.Log($"[Clear] 스테이지 돌파! {pdm.clearStageStep} → {clearedStageId}");
                pdm.clearStageStep = clearedStageId + 1;
                BackendGameData.Instance?.GameDataUpdateAsync();
            }
        }
    }
}