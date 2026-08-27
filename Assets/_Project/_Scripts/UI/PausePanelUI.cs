using Shield_Shot.Core.SceneFlow;
using Shield_Shot.GameplayCore.Monster.Stage;
using Shield_Shot.UI.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Fusion;
using Shield_Shot.GameplayCore.Network;

namespace Shield_Shot.UI
{
    public class PausePanelUI : MonoBehaviour
    {
        [SerializeField] private Button _resumeBtn;
        [SerializeField] private Button _retryBtn;
        [SerializeField] private Button _exitBtn;

        [SerializeField] private Button _audioBtn;
        [SerializeField] private Button _inputBtn;

        [SerializeField] private GameObject _audioPanel;
        [SerializeField] private GameObject _inputPanel;


        private void Awake()
        {
            _audioBtn.onClick.AddListener(() => SwitchSubPanel(true));
            _inputBtn.onClick.AddListener(() => SwitchSubPanel(false));
            if (_resumeBtn != null) _resumeBtn.onClick.AddListener(ResumeGame);
            if (_retryBtn != null) _retryBtn.onClick.AddListener(RetryGame);
            if (_exitBtn != null) _exitBtn.onClick.AddListener(ExitToLobby);
        }
        private void OnEnable()
        {
            Debug.Log("세팅창 활성화");
            SwitchSubPanel(true);
        }

        private void SwitchSubPanel(bool isAudio)
        {
            _audioPanel.SetActive(isAudio);
            _inputPanel.SetActive(!isAudio);
        }

        public void OpenPausePanel()
        {
            gameObject.SetActive(true);
            Time.timeScale = 0f;
            Debug.Log("[Pause] 게임 일시정지");
        }

        public void ResumeGame()
        {
            Time.timeScale = 1f;
            gameObject.SetActive(false);
            Debug.Log("[Pause] 게임 재개");
        }

        public void RetryGame()
        {

            Time.timeScale = 1f;

            var db = StageDatabase.Instance;

            Debug.Log($"[Retry] mode={db.Mode}");

            // SetStage(=SelectStory)는 모드를 Story로 바꿔버림 → 스토리 모드에서만 호출
            if (db.Mode == GameplayCore.Monster.Stage.GameMode.Story)
                db.SetStage(StageManager.Instance.CurrentStageIndex);
            else if (db.Mode == GameplayCore.Monster.Stage.GameMode.Infinite)
                db.SelectInfinite();
                // 무한 모드: _mode(Infinite)를 그대로 두고 재시작

                LoadStageScene(StageManager.Instance.CurrentStageIndex);
        }
        public async void ExitToLobby()
        {
            Time.timeScale = 1f;

            NetworkRunner runner = FindFirstObjectByType<NetworkRunner>();
            if (runner != null && runner.IsRunning)
            {
                if (NetworkMatchManager.Instance != null)
                {
                    await NetworkMatchManager.Instance.ShutdownNetworkAsync();
                }
                else
                {
                    await runner.Shutdown();
                }

                Debug.Log("[Pause] PvP 네트워크 종료 후 로비 화면 전환");
                SceneManager.LoadScene("02_Lobby");
                return;
            }

            SceneManager.LoadScene("03_Lobby");
            Debug.Log("[Pause] 로비 화면 전환");
        }

        //private void LoadStageScene(int stageIndex)
        //{
        //    // 1. 전달받은 인덱스를 기반으로 데이터베이스 상태를 강제로 일치시킴
        //    StageDatabase.Instance.SetStage(stageIndex);

        //    // 2. 데이터 세팅 (매니저의 상태를 참조하되, 방금 세팅한 인덱스와 일치하는지 확인)
        //    var data = new SceneTransitionData(SceneType.InGame, SceneType.Loading, SceneTransitionReason.Retry);

        //    // 매개변수로 받은 stageIndex가 곧 biomeStageIndex가 되도록 설계되어 있다면:
        //    data.Set("SelectedBiom", (int)StageManager.Instance.CurrentBiom);
        //    data.Set("BiomeStageIndex", stageIndex);

        //    SceneFlowManager.Instance.LoadScene("07_Loading", data);
        //}

        private void LoadStageScene(int stageIndex)
        {
            var db = StageDatabase.Instance;
            var data = new SceneTransitionData(SceneType.InGame, SceneType.Loading, SceneTransitionReason.Retry);

            if (db.Mode == GameplayCore.Monster.Stage.GameMode.Story)
            {
                db.SetStage(stageIndex);   // 스토리에서만 인덱스 세팅(=SelectStory)
                data.Set("SelectedBiom", (int)StageManager.Instance.CurrentBiom);
                data.Set("BiomeStageIndex", stageIndex);
            }
            else // Infinite
            {
                // 모드 건드리지 않음. 무한 모드임을 로딩 씬에 전달
                data.Set("Mode", (int)GameplayCore.Monster.Stage.GameMode.Infinite);
            }

            SceneFlowManager.Instance.LoadScene("07_Loading", data);
        }
    }

}

