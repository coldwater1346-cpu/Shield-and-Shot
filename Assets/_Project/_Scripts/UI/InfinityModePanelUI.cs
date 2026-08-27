using Shield_Shot.DataManagement;
using Shield_Shot.GameplayCore.Monster.Stage;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Shield_Shot.UI
{
    public class InfinityModePanelUI : MonoBehaviour
    {

        [SerializeField] private Button _enterBtn;
        [SerializeField] TextMeshProUGUI _highestWave;
        [SerializeField] private string _inGameSceneName = "04_InGame";
        [SerializeField] private PlayerDataManager _playerDataManager;

        private bool _isTransitioning = false;

        private void Awake()
        {
            _enterBtn.onClick.AddListener(OnClickEnter);

            if (_playerDataManager == null)
            {
                _playerDataManager = FindFirstObjectByType<PlayerDataManager>();
            }
        }

        private void OnEnable()
        {
            LoadRecord();
        }

        private void OnClickEnter()
        {
            if (_isTransitioning) return;

            Debug.Log($"[StagePanel] 무한 모드 스테이지 진입");

            StageDatabase.Instance.SelectInfinite();

            _playerDataManager.SavePlayerLoadData();

            EnterInGame();
        }

        private void EnterInGame()
        {
            _isTransitioning = true;
            SceneManager.LoadScene(_inGameSceneName);
        }

        private void LoadRecord()
        {
            int bestWave = 0;

            if (_playerDataManager != null)
            {
                bestWave =
                    _playerDataManager.highestWave;
            }

            if (bestWave <= 0)
            {
                _highestWave.text = "기록 없음";
            }
            else
            {
                _highestWave.text =
                    $"{bestWave} Wave";
            }
        }

    }
}

