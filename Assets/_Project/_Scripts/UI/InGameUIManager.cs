using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Shield_Shot.GameplayCore.Monster.Stage;

namespace Shield_Shot.UI
{
    public class InGameUIManager : MonoBehaviour
    {
        public static InGameUIManager Instance { get; private set; }

        [SerializeField] private Button _pauseBtn;
        [SerializeField] private Button _swapBtn;
        [SerializeField] private PausePanelUI _pausePanel;
        [SerializeField] private InGameTutorialGuideUI _tutorialGuideUI;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            if(_pauseBtn != null && _pausePanel != null)
            {
                _pauseBtn.onClick.AddListener(() => _pausePanel.OpenPausePanel());
            }
        }

        private IEnumerator Start()
        {
            yield return null;
            TryShowFirstStageTutorial();
        }

        private void TryShowFirstStageTutorial()
        {
            if (_tutorialGuideUI == null)
            {
                return;
            }

            StageDatabase stageDatabase = StageDatabase.Instance;
            if (stageDatabase == null ||
                stageDatabase.Mode != GameMode.Story ||
                stageDatabase.CurrentGlobalIndex != 0)
            {
                return;
            }

            _tutorialGuideUI.ShowIgnoringSeen();
        }
    }
}
