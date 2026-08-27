using System.Collections;
using Shield_Shot.GameplayCore.Monster.Stage;
using TMPro;
using UnityEngine;

namespace Shield_Shot.UI
{
    public class InGameStageInfo : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _stageText;
        [SerializeField] private TextMeshProUGUI _waveText;
        [SerializeField] private TextMeshProUGUI _monsterCountText;

        private void OnEnable()
        {
            UIEventBus.OnStageChanged += UpdateStageUI;
            UIEventBus.OnWaveChanged += UpdateWaveUI;
            UIEventBus.OnGoldChanged += UpdateGoldUI;
            UIEventBus.OnMonsterCountChanged += UpdateMonsterCountUI;

            StartCoroutine(InitializeUIAsync());
        }
        private void OnDisable()
        {
            UIEventBus.OnStageChanged -= UpdateStageUI;
            UIEventBus.OnWaveChanged -= UpdateWaveUI;
            UIEventBus.OnGoldChanged -= UpdateGoldUI;
            UIEventBus.OnMonsterCountChanged -= UpdateMonsterCountUI;
        }

        private void UpdateStageUI(int stage)
        {
            GameMode mode = StageDatabase.Instance.Mode;

            if(mode == GameMode.Story)
            {
                Debug.Log($"UI Update Called: {stage}");
                _stageText.text = $"스테이지 : {stage}";

                _waveText.gameObject.SetActive(false);
            }
            else
            {      
                _stageText.text = $"무한 모드";

                _waveText.gameObject.SetActive(true);
            }
        }
        private void UpdateGoldUI(int gold)
        {
            //_goldText.text = $"골드 : {gold}";
        }
        private void UpdateMonsterCountUI(int monsterCount)
        {
            _monsterCountText.text = $"몬스터 : {monsterCount}";
        }
        private void UpdateWaveUI(int currentWave)
        {
            _waveText.text = $"웨이브: {currentWave}";
        }

        private IEnumerator InitializeUIAsync()
        {
            while (StageDatabase.Instance == null)
            {
                yield return null;
            }
            UpdateStageUI(StageDatabase.Instance.CurrentStage + 1);

            // StageManager가 생성될 때까지 대기
            float timeOut = 3f; 
            float timer = 0f;

            while (StageManager.Instance == null)
            {
                timer += Time.deltaTime;
                if (timer > timeOut)
                {
                    Debug.LogError("[InGameStageInfo] StageManager Instance 없음 (시간 초과)");
                    yield break;
                }
                yield return null;
            }

            UpdateWaveUI(StageManager.Instance.CurrentWave);
        }
    }
}

