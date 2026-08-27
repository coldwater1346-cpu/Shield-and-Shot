using Shield_Shot.Core.SceneFlow;
using Shield_Shot.DataManagement;
using Shield_Shot.DataManagement.PlayerData;
using Shield_Shot.GameplayCore.Monster.Difficulty;
using Shield_Shot.UI.Core;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Shield_Shot.UI
{
    public class StageSelectPanelUI : MonoBehaviour
    {
        [SerializeField] private Button _nextBtn;
        [SerializeField] private Button _enterBtn;
        [SerializeField] private Button _prevBtn;
        [SerializeField] private Button _chapterBtn;
        [SerializeField] private string _inGameSceneName = "04_InGame";

        [SerializeField] private PlayerDataManager _playerDataManager;

        [SerializeField] private Image _mainDisplay;
        [SerializeField] private GameObject _prevSlot;
        [SerializeField] private GameObject _nextSlot;
        [SerializeField] private TextMeshProUGUI _prevText;
        [SerializeField] private TextMeshProUGUI _currentText;
        [SerializeField] private TextMeshProUGUI _nextText;
        [SerializeField] private GameObject _lockMain;
        [SerializeField] private GameObject _lockSlot1;
        [SerializeField] private GameObject _lockSlot2;
        [SerializeField] private GameObject _lockSlot3;

        [SerializeField] private List<ChapterSO> _chapterData;
        [SerializeField] private List<Sprite> _chapterSprites;

        private int _maxClearIdx;
        private bool _isLocked;

        public List<Sprite> GetChapterSprites()
        {
            return _chapterSprites;
        }

        //[SerializeField] private int _maxStageCount = 5;
        [SerializeField] private int _currentIndex = 0;
        [SerializeField] private int _totalStages;
        private bool _isTransitioning = false;

        private void Awake()
        {
            _totalStages = 0;
            foreach (var chapter in _chapterData)
            {
                _totalStages += chapter.StageCount;
            }

            _prevBtn.onClick.AddListener(OnClickPrev);
            _nextBtn.onClick.AddListener(OnClickNext);
            _enterBtn.onClick.AddListener(OnClickEnter);
            _chapterBtn.onClick.AddListener(() => UIManager.Instance.ShowPopup("Chapter"));

            if (_playerDataManager == null)
            {
                _playerDataManager = FindFirstObjectByType<PlayerDataManager>();
            }


        }


        void Start()
        {
            UpdateStageUI();
        }

        private void OnClickEnter()
        {
            if (_isTransitioning) return;

            _maxClearIdx = _playerDataManager.clearStageStep;
            _isLocked = _currentIndex > _maxClearIdx;

            // 1. 현재 스테이지가 잠겼는지 체크
            if (_isLocked)
            {
                // 2. 잠겨있다면 AlertSystem의 Toast 호출
                AlertSystem.ShowToast("아직 잠긴 스테이지입니다!\n이전 스테이지를 클리어 필요.", 2f);
                return; // 입장 로직 중단
            }

            // 3. 잠기지 않았다면 정상 입장
            var (biom, biomeStageIndex) = GetCurrentStageInfo();
            Debug.Log($"[StagePanel] {biom} 챕터, {biomeStageIndex + 1} 스테이지 진입");

            _playerDataManager.SavePlayerLoadData();

            var lobbyController = FindFirstObjectByType<LobbySceneController>();
            if (lobbyController != null)
            {
                _isTransitioning = true;
                // 이제 인덱스 대신 챕터와 스테이지 정보를 넘깁니다.
                lobbyController.OnStartStageClicked(biom, biomeStageIndex);
            }
        }        

        private void OnClickPrev()
        {
            if (_currentIndex > 0)
            {
                _currentIndex--;
                UpdateStageUI();
            }
        }
        private void OnClickNext()
        {
            if (_currentIndex < _totalStages - 1)
            {
                _currentIndex++;
                UpdateStageUI();
            }
        }
        private void UpdateStageUI()
        {
            int tempIndex = _currentIndex;
            int chapterIndex = 0;
            int stageInChapter = 0;

            _maxClearIdx = _playerDataManager.clearStageStep;
            _isLocked = _currentIndex > _maxClearIdx;
            bool _isLockedPrev = (_currentIndex - 1) > _maxClearIdx;
            bool _isLockedNext = (_currentIndex + 1) > _maxClearIdx;

            if (_lockSlot2 != null || _lockMain != null)
            {
                _lockSlot2.SetActive(_isLocked);
                _lockMain.SetActive(_isLocked);
            }
            if (_lockSlot1 != null) _lockSlot1.SetActive(_isLockedPrev && (_currentIndex > 0));
            if (_lockSlot3 != null) _lockSlot3.SetActive(_isLockedNext && (_currentIndex < _totalStages - 1));

            // 현재 인덱스가 어느 챕터의 몇 번째 스테이지인지 찾기
            for (int i = 0; i < _chapterData.Count; i++)
            {
                if (tempIndex < _chapterData[i].StageCount)
                {
                    chapterIndex = i;
                    stageInChapter = tempIndex + 1;
                    break;
                }
                tempIndex -= _chapterData[i].StageCount;
            }

            // 1. 메인 배경 업데이트
            if (chapterIndex < _chapterSprites.Count)
                _mainDisplay.sprite = _chapterSprites[chapterIndex];

            // 2. 현재 스테이지 번호
            _currentText.text = stageInChapter.ToString();

            // 3. 이전 슬롯 (현재 챕터 내의 이전 번호 혹은 이전 챕터의 마지막 번호 계산 필요)
            bool hasPrev = _currentIndex > 0;
            _prevSlot.gameObject.SetActive(hasPrev);
            if (hasPrev)
            {
                // 이전 스테이지 인덱스 구하기
                int prevIdx = _currentIndex - 1;
                int pTemp = prevIdx;
                int pChapter = 0;
                for (int i = 0; i < _chapterData.Count; i++)
                {
                    if (pTemp < _chapterData[i].StageCount) { pChapter = i; break; }
                    pTemp -= _chapterData[i].StageCount;
                }
                _prevText.text = (pTemp + 1).ToString();
            }

            // 4. 다음 슬롯
            bool hasNext = _currentIndex < _totalStages - 1;
            _nextSlot.gameObject.SetActive(hasNext);
            if (hasNext)
            {
                // 다음 스테이지 인덱스 구하기
                int nextIdx = _currentIndex + 1;
                int nTemp = nextIdx;
                int nChapter = 0;
                for (int i = 0; i < _chapterData.Count; i++)
                {
                    if (nTemp < _chapterData[i].StageCount) { nChapter = i; break; }
                    nTemp -= _chapterData[i].StageCount;
                }
                _nextText.text = (nTemp + 1).ToString();
            }

            _prevBtn.interactable = hasPrev;
            _nextBtn.interactable = hasNext;
        }

        public void SetChapter(int targetChapterIndex)
        {
            int newIndex = 0;
            // 선택한 챕터 이전까지의 모든 스테이지 수를 더함
            for (int i = 0; i < targetChapterIndex; i++)
            {
                newIndex += _chapterData[i].StageCount;
            }

            _currentIndex = newIndex;
            UpdateStageUI();
        }

        public (ChapterBiom biom, int biomeStageIndex) GetCurrentStageInfo()
        {
            int tempIndex = _currentIndex;

            for (int i = 0; i < _chapterData.Count; i++)
            {
                // tempIndex가 현재 챕터의 스테이지 수보다 작으면 해당 챕터가 맞음
                if (tempIndex < _chapterData[i].StageCount)
                {
                    return (_chapterData[i].Biom, tempIndex); // biomeStageIndex는 0부터 시작
                }
                // 아니라면 다음 챕터로 넘어가기 위해 빼줌
                tempIndex -= _chapterData[i].StageCount;
            }

            // 루프를 다 돌았는데도 못 찾았다면 기본값 반환
            Debug.LogError($"[StagePanel] 인덱스 오류! 현재 인덱스: {_currentIndex}");
            return (_chapterData[0].Biom, 0);
        }
    }
}
