using Shield_Shot.GameplayCore.Monster.Difficulty;
using Shield_Shot.GameplayCore.Network;
using Shield_Shot.GameplayCore.Network.Match;
using Shield_Shot.NetworkCore;
using Shield_Shot.UI.Matchmaking;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Shield_Shot.Core.SceneFlow
{
    public sealed class LobbySceneController : BaseSceneController
    {
        [Header("Buttons")]
        [SerializeField] private Button _quickMatchButton;

        [Header("UI")]
        [SerializeField] private MatchmakingPanelUI _matchmakingPanelUI;

        private readonly List<Button> _boundQuickMatchButtons = new();

        protected override void OnEnterScene(SceneTransitionData transitionData)
        {
            Time.timeScale = 1f;

            BindButtons();
            BindNetworkEvents();

            if (_matchmakingPanelUI != null)
            {
                _matchmakingPanelUI.Hide();
            }

            SetQuickMatchButtonsInteractable(true);
        }

        protected override void OnExitScene()
        {
            UnbindButtons();
            UnbindNetworkEvents();
        }

        private void BindButtons()
        {
            _boundQuickMatchButtons.Clear();
            AddQuickMatchButton(_quickMatchButton);

            Button[] buttons = FindObjectsByType<Button>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

            for (int i = 0; i < buttons.Length; i++)
            {
                Button button = buttons[i];
                if (button == null)
                {
                    continue;
                }

                string buttonName = button.gameObject.name;
                if (buttonName.Contains("QuickMatching") ||
                    buttonName.Contains("QuickMatch"))
                {
                    AddQuickMatchButton(button);
                }
            }

            if (_matchmakingPanelUI != null && _matchmakingPanelUI.CancelButton != null)
            {
                _matchmakingPanelUI.CancelButton.onClick.RemoveListener(OnCancelMatchmakingButtonClicked);
                _matchmakingPanelUI.CancelButton.onClick.AddListener(OnCancelMatchmakingButtonClicked);
            }

            Debug.Log($"[LobbySceneController] Quick match buttons bound: {_boundQuickMatchButtons.Count}");
        }

        private void UnbindButtons()
        {
            for (int i = 0; i < _boundQuickMatchButtons.Count; i++)
            {
                if (_boundQuickMatchButtons[i] != null)
                {
                    _boundQuickMatchButtons[i].onClick.RemoveListener(OnQuickMatchButtonClicked);
                }
            }

            _boundQuickMatchButtons.Clear();

            if (_matchmakingPanelUI != null && _matchmakingPanelUI.CancelButton != null)
            {
                _matchmakingPanelUI.CancelButton.onClick.RemoveListener(OnCancelMatchmakingButtonClicked);
            }
        }

        private void AddQuickMatchButton(Button button)
        {
            if (button == null || _boundQuickMatchButtons.Contains(button))
            {
                return;
            }

            button.onClick.RemoveListener(OnQuickMatchButtonClicked);
            button.onClick.AddListener(OnQuickMatchButtonClicked);
            _boundQuickMatchButtons.Add(button);
        }

        private void BindNetworkEvents()
        {
            if (NetworkMatchManager.Instance == null)
            {
                Debug.LogWarning("[LobbySceneController] NetworkMatchManager instance is missing.");
                return;
            }

            NetworkMatchManager.Instance.MatchmakingStarted -= OnMatchmakingStarted;
            NetworkMatchManager.Instance.QuickMatchSucceeded -= OnQuickMatchSucceeded;
            NetworkMatchManager.Instance.QuickMatchFailed -= OnQuickMatchFailed;
            NetworkMatchManager.Instance.QuickMatchCanceled -= OnQuickMatchCanceled;

            NetworkMatchManager.Instance.MatchmakingStarted += OnMatchmakingStarted;
            NetworkMatchManager.Instance.QuickMatchSucceeded += OnQuickMatchSucceeded;
            NetworkMatchManager.Instance.QuickMatchFailed += OnQuickMatchFailed;
            NetworkMatchManager.Instance.QuickMatchCanceled += OnQuickMatchCanceled;
        }

        private void UnbindNetworkEvents()
        {
            if (NetworkMatchManager.Instance == null)
            {
                return;
            }

            NetworkMatchManager.Instance.MatchmakingStarted -= OnMatchmakingStarted;
            NetworkMatchManager.Instance.QuickMatchSucceeded -= OnQuickMatchSucceeded;
            NetworkMatchManager.Instance.QuickMatchFailed -= OnQuickMatchFailed;
            NetworkMatchManager.Instance.QuickMatchCanceled -= OnQuickMatchCanceled;
        }

        private void OnQuickMatchButtonClicked()
        {
            Debug.Log("[LobbySceneController] Quick match button clicked.");

            if (NetworkMatchManager.Instance == null)
            {
                Debug.LogError("[LobbySceneController] NetworkMatchManager instance is missing.");
                OnQuickMatchFailed("Network manager is missing.");
                return;
            }

            NetworkMatchManager.Instance.StartQuickMatch();
        }

        private void OnCancelMatchmakingButtonClicked()
        {
            if (NetworkMatchManager.Instance == null)
            {
                OnQuickMatchCanceled();
                return;
            }

            NetworkMatchManager.Instance.CancelQuickMatch();
        }

        private void OnMatchmakingStarted()
        {
            SetQuickMatchButtonsInteractable(false);

            if (_matchmakingPanelUI != null)
            {
                _matchmakingPanelUI.ShowSearching();
            }
        }

        private void OnQuickMatchSucceeded(MatchContext matchContext)
        {
            if (_matchmakingPanelUI != null)
            {
                _matchmakingPanelUI.ShowFound();
            }
        }

        private void OnQuickMatchFailed(string errorMessage)
        {
            if (_matchmakingPanelUI != null)
            {
                _matchmakingPanelUI.ShowFailed(errorMessage);
            }

            SetQuickMatchButtonsInteractable(true);
        }

        private void OnQuickMatchCanceled()
        {
            if (_matchmakingPanelUI != null)
            {
                _matchmakingPanelUI.ShowCanceled();
            }

            SetQuickMatchButtonsInteractable(true);
        }

        private void SetQuickMatchButtonsInteractable(bool isInteractable)
        {
            for (int i = 0; i < _boundQuickMatchButtons.Count; i++)
            {
                if (_boundQuickMatchButtons[i] != null)
                {
                    _boundQuickMatchButtons[i].interactable = isInteractable;
                }
            }
        }

        // 스테이지 선택 로직
        public void OnStartStageClicked(ChapterBiom selectedBiom,int stageIndex)
        {
            //서버 등록
            //BackendManager.Instance.SelectStage(stageIndex);

            var transitionData = new SceneTransitionData(SceneType.Lobby, SceneType.Loading, SceneTransitionReason.LobbyToInGame);
            transitionData.Set("SelectedBiom", (int)selectedBiom);
            transitionData.Set("BiomeStageIndex", stageIndex);

            SceneFlowManager.Instance.LoadScene("07_Loading", transitionData);
        }
    }
}
