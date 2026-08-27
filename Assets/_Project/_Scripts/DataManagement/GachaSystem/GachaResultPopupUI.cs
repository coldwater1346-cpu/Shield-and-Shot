using Shield_Shot.DataManagement.GachaSystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Shield_Shot.DataManagement.GachaSystem
{
    public class GachaResultPopupUI : MonoBehaviour
    {
        [SerializeField] private Transform _content;
        [SerializeField] private GachaResultCardUI _cardPrefab;
        [SerializeField] private GameObject _gachaResultPopupUI;
        [SerializeField] private Button _closeButton;

        [SerializeField] private ScrollRect _scrollRect;
        // 모두 공개 버튼 
        [SerializeField] private Button _openAllButton;

        [Header("사운드 클립")]
        [SerializeField] private AudioClip _gachaClip; //  뽑기 효과음 

        private List<GachaResultCardUI> _spawnedCards = new List<GachaResultCardUI>();

        private void Awake()
        {
            if(_closeButton != null)
            _closeButton.onClick.AddListener(Close);

            if (_openAllButton != null)
                _openAllButton.onClick.AddListener(OpenAllCards);
        }


        private void OnDestroy()
        {
            if(_closeButton != null)
            _closeButton.onClick.RemoveListener(Close);

            if (_openAllButton != null)
                _openAllButton.onClick.RemoveListener(OpenAllCards);
        }

        private void Update()
        {
            // 팝업창이 켜져 있고, 버튼이 아직 활성화 상태일 때
            if (_gachaResultPopupUI.activeSelf && _openAllButton != null && _openAllButton.interactable)
            {
                // 만약 모든 카드가 열렸다면 버튼을 비활성화
                if (CheckAllCardsOpened())
                {
                    _openAllButton.interactable = false;
                }
            }
        }

        /// <summary>
        /// 리스트에 있는 모든 카드가 열렸는지 검사
        /// </summary>
        private bool CheckAllCardsOpened()
        {
           
            if (_spawnedCards.Count == 0) return false;

            foreach (var card in _spawnedCards)
            {
                if (card != null)
                {
                    
                    if (!card.IsOpened)
                        return false;
                }
            }

            //  안 열린 카드가 없다면 true 
            return true;
        }
        public void Open(
     List<GachaController.GachaResultData> results)
        {
            if (_gachaResultPopupUI == null)
            {
                Debug.LogError(
                    "[GachaResultPopupUI] 결과 팝업이 연결되지 않았습니다.");

                return;
            }

            // 이전 결과 카드 정리
            ClearCards();

            _gachaResultPopupUI.SetActive(true);

            if (_openAllButton != null)
            {
                _openAllButton.gameObject.SetActive(true);
                _openAllButton.interactable = true;
            }

            foreach (var result in results)
            {
                GachaResultCardUI card =
                    Instantiate(_cardPrefab, _content);

                card.SetData(result);
                _spawnedCards.Add(card);
            }

            ResetScrollPosition();
        }

        //  한번에 공개 버튼을 눌렀을 때
        public void OpenAllCards()
        {
            // 태어난 모든 카드들을 한 바퀴 돌면서 강제로 오픈
            foreach (var card in _spawnedCards)
            {
                if (card != null)
                {
                    card.ForceOpenCard();
                }
            }

            // 가차 사운드 재생
            if (_gachaClip != null)
            {
                Shield_Shot.Audio.SoundManager.Instance.PlayUI(_gachaClip, 0.1f);
            }

            // 중복 클릭 방지 (버튼 비활성화)
            if (_openAllButton != null)
            {
                _openAllButton.interactable = false; 
            }
           
        }

        public void Close()
        {
            if (_scrollRect != null)
            {
                _scrollRect.StopMovement();
                _scrollRect.velocity = Vector2.zero;
            }

            ClearCards();

            if (_gachaResultPopupUI != null)
            {
                _gachaResultPopupUI.SetActive(false);
            }
        }

        private void ResetScrollPosition()
        {
            if (_scrollRect == null)
            {
                Debug.LogWarning(
                    "[GachaResultPopupUI] ScrollRect가 연결되지 않았습니다.");

                return;
            }

            // 생성된 카드에 맞춰 Content 크기를 즉시 다시 계산
            Canvas.ForceUpdateCanvases();

            if (_content is RectTransform contentRect)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(
                    contentRect);
            }

            Canvas.ForceUpdateCanvases();

            // 이전 스크롤 관성 제거
            _scrollRect.StopMovement();
            _scrollRect.velocity = Vector2.zero;

            // 가장 왼쪽으로 이동
            _scrollRect.horizontalNormalizedPosition = 0f;
        }
        private void ClearCards()
        {
            foreach (GachaResultCardUI card in _spawnedCards)
            {
                if (card == null)
                {
                    continue;
                }

                // 레이아웃에서 즉시 제외
                card.gameObject.SetActive(false);

                // 실제 오브젝트는 프레임 종료 시 삭제
                Destroy(card.gameObject);
            }

            _spawnedCards.Clear();
        }
    }
}