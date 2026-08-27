using Shield_Shot.Core.SceneFlow;
using Shield_Shot.DataManagement;
using Shield_Shot.GameplayCore.Monster.Difficulty;
using Shield_Shot.GameplayCore.Monster.Stage;
using Shield_Shot.NetworkCore;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Shield_Shot.UI
{
    public class ResultPanelUI : MonoBehaviour
    {
        [Header("Result Group State")]
        [SerializeField] private GameObject _victoryGroup;
        [SerializeField] private GameObject _defeatGroup;

        [Header("Victory Panel Buttons")]
        [SerializeField] private Button _vicNextBtn;
        [SerializeField] private Button _vicRetryBtn;
        [SerializeField] private Button _vicExitBtn;
        [SerializeField] private TextMeshProUGUI _resultGold_1;
        [SerializeField] private TextMeshProUGUI _killCount_1;


        [Header("Defeat Panel Buttons")]
        [SerializeField] private Button _defReplayBtn;
        [SerializeField] private Button _defRetryBtn;
        [SerializeField] private Button _defExitBtn;
        [SerializeField] private TextMeshProUGUI _resultGold_2;
        [SerializeField] private TextMeshProUGUI _killCount_2;
        [SerializeField] private TextMeshProUGUI _clearWave;

        [Header("Player Data")]
        [SerializeField] private PlayerDataManager _playerDataManager;

        private void Awake()
        {
            if (_vicNextBtn != null) _vicNextBtn.onClick.AddListener(GoToNextStage);
            if (_vicRetryBtn != null) _vicRetryBtn.onClick.AddListener(RetryStage);
            if (_vicExitBtn != null) _vicExitBtn.onClick.AddListener(ExitToLobby);

            if (_defReplayBtn != null) _defReplayBtn.onClick.AddListener(OnReplayADClick);
            if (_defRetryBtn != null) _defRetryBtn.onClick.AddListener(RetryStage);
            if (_defExitBtn != null) _defExitBtn.onClick.AddListener(ExitToLobby);

            if(_playerDataManager == null)
                _playerDataManager= FindFirstObjectByType<PlayerDataManager>();
        }

        // 게임매니저에서 승패 판정시 호출
        public void ShowResult(bool isVictory)
        {
            gameObject.SetActive(true);

            _victoryGroup.SetActive(isVictory);
            _defeatGroup.SetActive(!isVictory);

            if(StageManager.Instance != null)
            {
                GameMode mode = StageDatabase.Instance.Mode;

                if(mode == GameMode.Story)
                {
                    int resultGold = isVictory ? StageManager.Instance.RewardGold : 0;
                    UpdateResultGold(resultGold);
                    _clearWave.gameObject.SetActive(false);
                }
                else
                {
                    _clearWave.gameObject.SetActive(true);
                    int resultGold = StageManager.Instance.RewardGold;
                    UpdateResultGold(resultGold);
                }

                int resultKillCount = StageManager.Instance.KillCount;
                int resultClearWave = StageManager.Instance.CurrentWave - 1;
                UpdateResultKillCount(resultKillCount);
                UpdateClearWave(resultClearWave);
                CheckWaveRecord(resultClearWave);
            }

            Time.timeScale = 0f;
            Debug.Log(isVictory ? "[Result] 승리" : "[Result] 패배");
        }

        private void RetryStage()
        {
            Time.timeScale = 1f;

            // 1. 현재 정보를 매니저에서 가져와 데이터베이스 갱신
            int currentIndex = StageManager.Instance.CurrentStageIndex;
            StageDatabase.Instance.SetStage(currentIndex);

            // 2. 로딩 씬 호출 (매니저의 현재 상태 기반)
            LoadStageScene(currentIndex);

            Debug.Log("[Result] 스테이지 재시작");
        }

        private void OnReplayADClick()
        {
            Debug.Log("[AD] 광고 요청");

            OnADShowComplete();
        }

        private void OnADShowComplete()
        {
            Debug.Log("[AD] 광고 시청 완료, 플레이어 부활");

            //Time.timeScale = 1f;
            gameObject.SetActive(false);

            // TODO: 현재 상태에서 부활 로직
        }

        private void ExitToLobby()
        {
            Time.timeScale = 1f;
            SceneFlowManager.Instance.LoadScene("03_Lobby", null);

            Debug.Log("[Result] 로비 씬 전환");
        }

        private void GoToNextStage()
        {
            Time.timeScale = 1f;

            // 1. 다음 스테이지 인덱스 계산
            int nextIndex = StageDatabase.Instance.CurrentStage + 1;

            if (nextIndex < StageDatabase.Instance.Count)
            {
                // 2. 데이터베이스 갱신 및 로드
                StageDatabase.Instance.SetStage(nextIndex);
                LoadStageScene(nextIndex);
            }
            else
            {
                Debug.Log("[Result] 마지막 스테이지입니다.");
                ExitToLobby();
            }

            Debug.Log("[Result] 다음 스테이지 진입");
        }

        private void LoadStageScene(int stageIndex)
        {
            // 1. 전달받은 인덱스를 기반으로 데이터베이스 상태를 강제로 일치시킴
            StageDatabase.Instance.SetStage(stageIndex);

            // 2. 데이터 세팅 (매니저의 상태를 참조하되, 방금 세팅한 인덱스와 일치하는지 확인)
            var data = new SceneTransitionData(SceneType.InGame, SceneType.Loading, SceneTransitionReason.Retry);

            // 매개변수로 받은 stageIndex가 곧 biomeStageIndex가 되도록 설계되어 있다면:
            data.Set("SelectedBiom", (int)StageManager.Instance.CurrentBiom);
            data.Set("BiomeStageIndex", stageIndex);

            SceneFlowManager.Instance.LoadScene("07_Loading", data);
        }

        private void UpdateResultGold(int gold)
        {
            _resultGold_1.text = $"획득 골드: {gold}";
            _resultGold_2.text = $"획득 골드: {gold}";
        }

        private void UpdateResultKillCount(int killCount)
        {
            _killCount_1.text = $"처치 수: {killCount}";
            _killCount_2.text = $"처치 수: {killCount}";
        }

        private void UpdateClearWave(int clearWave)
        {
            _clearWave.text = $"클리어 웨이브: {clearWave}";
        }

        private void CheckWaveRecord(int currentWave)
        {
            // 스토리 모드 기록을 최고 웨이브로 저장하지 않음
            if (StageDatabase.Instance == null ||
                StageDatabase.Instance.Mode != GameMode.Infinite)
            {
                return;
            }

            if (_playerDataManager == null)
            {
                Debug.LogError(
                    "[ResultPanelUI] PlayerDataManager가 없어 " +
                    "최고 웨이브를 저장할 수 없습니다.");

                return;
            }

            if (currentWave <= _playerDataManager.highestWave)
                return;

            _playerDataManager.highestWave = currentWave;

            Debug.Log(
                $"[ResultPanelUI] 무한 모드 신기록: " +
                $"{currentWave} Wave");

           
        }
    }
}

