using Shield_Shot.UI.Core;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Shield_Shot.UI
{
    public class SelectChapterPopupUI : UIPopupBase
    {
        [SerializeField] private StageSelectPanelUI _mainStagePanel;
        [SerializeField] private Image _prevChapterImg;
        [SerializeField] private Image _currentChapterImg;
        [SerializeField] private Image _nextChapterImg;
        [SerializeField] private Button _prevBtn;
        [SerializeField] private Button _nextBtn;
        [SerializeField] private Button _selectBtn;
        [SerializeField] private Button _closeBtn;

        private List<Sprite> ChapterSprites => _mainStagePanel.GetChapterSprites();

        private int _selectedChapterIndex = 0;

        private void Awake()
        {
            _prevBtn.onClick.AddListener(() => ChangeSelection(-1));
            _nextBtn.onClick.AddListener(() => ChangeSelection(1));
            _selectBtn.onClick.AddListener(OnConfirmSelect);
            _closeBtn.onClick.AddListener(() => UIManager.Instance.ClosePopup("Chapter"));
        }

        private void OnEnable()
        {
            // 열릴 때 현재 스테이지 패널의 챕터를 기준으로 초기화
            // (간단히 0번부터 시작하게 하려면 0으로 세팅)
            _selectedChapterIndex = 0;
            UpdateUI();
        }

        private void ChangeSelection(int delta)
        {
            int maxIndex = ChapterSprites.Count - 1;

            _selectedChapterIndex = Mathf.Clamp(_selectedChapterIndex + delta, 0, maxIndex);
            UpdateUI();
        }

        private void UpdateUI()
        {
            var sprites = ChapterSprites;
            _currentChapterImg.sprite = sprites[_selectedChapterIndex];

            // 좌우 슬롯 활성화 및 이미지 설정
            bool hasPrev = _selectedChapterIndex > 0;
            _prevChapterImg.transform.parent.gameObject.SetActive(hasPrev); // 슬롯 오브젝트 제어
            if (hasPrev) _prevChapterImg.sprite = sprites[_selectedChapterIndex - 1];

            bool hasNext = _selectedChapterIndex < sprites.Count - 1;
            _nextChapterImg.transform.parent.gameObject.SetActive(hasNext);
            if (hasNext) _nextChapterImg.sprite = sprites[_selectedChapterIndex + 1];
        }

        private void OnConfirmSelect()
        {
            _mainStagePanel.SetChapter(_selectedChapterIndex);
            UIManager.Instance.ClosePopup("Chapter");
        }
    }
}


