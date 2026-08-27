using System;
using System.Collections.Generic;
using Shield_Shot.GameplayCore.Weapon.Projectile;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // ★ DoTween 네임스페이스 추가
using Shield_Shot.GameplayCore.Network.Pvp;

namespace Shield_Shot.GameplayCore.Augment
{
    public class AugmentPopupUI : MonoBehaviour
    {
        /// <summary>
        /// 증강 선택이 끝났을 때(카드 선택 완료, 포기, 또는 뽑을 게 없어서 자동 스킵) 발생한다.
        /// StageManager 등 "선택이 끝나야 다음으로 진행"해야 하는 곳에서 구독해서 사용한다.
        /// </summary>
        public event Action OnAugmentSelectionCompleted;

        [Header("인게임 카드 UI 슬롯 목록")]
        [SerializeField] private List<AugmentCardUI> cardSlots = new List<AugmentCardUI>();

        [Header("팝업 전역 페이드 컴포넌트")]
        [SerializeField] private CanvasGroup popupCanvasGroup;

        [Header("리롤")]
        [Tooltip("리롤 버튼. 한 번의 증강 뽑기 세션(팝업이 열려있는 동안)에 딱 1번만 사용 가능하다.")]
        [SerializeField] private Button _rerollButton;

        [Header("포기")]
        [Tooltip("증강 포기 버튼. 아무 특성도 업그레이드하지 않고 바로 팝업을 닫는다.")]
        [SerializeField] private Button _skipButton;

        [Header("PvP")]
        [SerializeField] private PvpAugmentSelectionBridge _pvpSelectionBridge;

        private bool _isOpening;
        private bool _hasRerolledThisSession;
        
        public static bool IsOpen { get; private set; }

        private void Awake()
        {
            if (popupCanvasGroup == null) popupCanvasGroup = GetComponent<CanvasGroup>();

            if (!_isOpening)
            {
                gameObject.SetActive(false);
            }
            if (_pvpSelectionBridge == null)
            {
                _pvpSelectionBridge = FindFirstObjectByType<PvpAugmentSelectionBridge>();
            }
        }

        public void OpenPopup()
        {
            if (gameObject.activeInHierarchy)
            {
                return;
            }

            if (AugmentManager.Instance == null)
            {
                Debug.LogError("[AugmentPopupUI] AugmentManager 인스턴스를 찾을 수 없어 팝업을 열 수 없습니다.");
                return;
            }

            List<ProjectileBehaviorSO> rolledData = AugmentManager.Instance.RollAugments();

            if (rolledData == null)
            {
                // 뽑을 수 있는 특성이 하나도 없음(전부 마스터 레벨) - 팝업을 띄우지 않고
                // 곧바로 "선택 완료"로 처리해서 진행이 막히지 않게 한다.
                Debug.Log("[AugmentPopupUI] 뽑을 수 있는 증강이 없어 팝업을 스킵하고 바로 진행합니다.");
                _pvpSelectionBridge?.NotifySelectionCompleted();
                OnAugmentSelectionCompleted?.Invoke();
                return;
            }

            _hasRerolledThisSession = false;
            if (_rerollButton != null)
            {
                _rerollButton.onClick.RemoveAllListeners();
                _rerollButton.onClick.AddListener(OnRerollClicked);
                _rerollButton.interactable = true;
            }

            if (_skipButton != null)
            {
                _skipButton.onClick.RemoveAllListeners();
                _skipButton.onClick.AddListener(OnSkipClicked);
                _skipButton.interactable = true;
            }

            _isOpening = true;
            ActivateInactiveParents();
            gameObject.SetActive(true);
            _isOpening = false;

            if (!gameObject.activeInHierarchy)
            {
                Debug.LogWarning("[AugmentPopupUI] 팝업 오브젝트가 아직 Hierarchy에서 활성화되지 않아 화면에 표시할 수 없습니다.");
                return;
            }

            try
            {
                // 팝업 알파 초기화 후 부드럽게 창 오픈
                if (popupCanvasGroup != null)
                {
                    popupCanvasGroup.alpha = 0f;
                    popupCanvasGroup.blocksRaycasts = true;
                }

                PopulateCards(rolledData);

                if (popupCanvasGroup != null)
                {
                    popupCanvasGroup.DOFade(1f, 0.2f).SetUpdate(true);
                }

                // UI 준비가 끝난 뒤에만 인게임 시간 정지
                Time.timeScale = 0f;
                IsOpen = true;
            }
            catch (System.Exception exception)
            {
                _isOpening = false;
                Time.timeScale = 1f;
                gameObject.SetActive(false);
                Debug.LogException(exception);
            }
        }

        /// <summary>
        /// 뽑힌 데이터로 카드 슬롯들을 채운다. 최초 오픈과 리롤 둘 다 이 메서드를 공유한다.
        /// </summary>
        private void PopulateCards(List<ProjectileBehaviorSO> rolledData)
        {
            int slotCount = cardSlots.Count;
            for (int i = 0; i < slotCount; i++)
            {
                if (cardSlots[i] == null) continue;

                ProjectileBehaviorSO data = (i < rolledData.Count) ? rolledData[i] : null;
                int currentLvl = 0;

                if (data != null)
                {
                    currentLvl = AugmentManager.Instance.GetCurrentLevel(data.BehaviorID);
                }

                cardSlots[i].SetupUICardSlot(data, i + 1, currentLvl, OnCardSelected);
            }
        }

        /// <summary>
        /// 리롤 버튼 클릭 시 호출된다. 증강 뽑기 세션(팝업이 한 번 열려있는 동안)당 딱 1번만 허용한다.
        /// 팝업은 닫지 않고 카드만 다시 뽑아서 같은 슬롯을 재구성한다.
        /// </summary>
        private void OnRerollClicked()
        {
            if (_hasRerolledThisSession)
            {
                Debug.Log("[AugmentPopupUI] 이미 이번 뽑기에서 리롤을 사용했습니다.");
                return;
            }

            if (AugmentManager.Instance == null) return;

            List<ProjectileBehaviorSO> rerolledData = AugmentManager.Instance.RollAugments();

            if (rerolledData == null)
            {
                // 리롤하려는 시점에 마침 뽑을 게 하나도 안 남아있는 극단적인 경우.
                // 기존 카드를 그대로 두고 리롤만 취소한다 (팝업은 안 닫음).
                Debug.LogWarning("[AugmentPopupUI] 리롤할 증강이 없어 기존 카드를 유지합니다.");
                return;
            }

            _hasRerolledThisSession = true;
            if (_rerollButton != null)
                _rerollButton.interactable = false;

            PopulateCards(rerolledData);

            Debug.Log("[AugmentPopupUI] 리롤 완료.");
        }

        /// <summary>
        /// 포기 버튼 클릭 시 호출된다. 아무 특성도 업그레이드하지 않고,
        /// 카드를 전부 페이드아웃시킨 뒤 선택 완료 신호를 보내고 팝업을 닫는다.
        /// 카드 선택과 마찬가지로 진행(스테이지/매치)이 막히지 않도록 반드시 완료 신호를 보낸다.
        /// </summary>
        private void OnSkipClicked()
        {
            Debug.Log("[AugmentPopupUI] 증강 포기를 선택했습니다.");

            // 중복 클릭 방지
            if (popupCanvasGroup != null) popupCanvasGroup.blocksRaycasts = false;
            if (_rerollButton != null) _rerollButton.interactable = false;
            if (_skipButton != null) _skipButton.interactable = false;

            for (int i = 0; i < cardSlots.Count; i++)
            {
                if (cardSlots[i] != null)
                    cardSlots[i].PlayFadeOutSequence();
            }

            _pvpSelectionBridge?.NotifySelectionCompleted();
            ClosePopup();
        }

        private void ActivateInactiveParents()
        {
            List<GameObject> inactiveParents = new List<GameObject>();
            Transform current = transform.parent;

            while (current != null)
            {
                if (!current.gameObject.activeSelf)
                {
                    inactiveParents.Add(current.gameObject);
                }

                current = current.parent;
            }

            for (int i = inactiveParents.Count - 1; i >= 0; i--)
            {
                inactiveParents[i].SetActive(true);
            }
        }

        /// <summary>
        /// 카드가 선택되었을 때 실행되는 연출 및 데이터 업그레이드 오케스트라 함수
        /// </summary>
        private void OnCardSelected(int slotIndex, string behaviorID)
        {
            string slotPositionName = slotIndex == 1 ? "왼쪽(위)" : "오른쪽(아래)";
            Debug.Log($"<color=yellow>📢 [Console Log] 인덱스 {slotIndex}번({slotPositionName}) 슬롯의 [{behaviorID}] 카드를 선택했습니다!</color>");

            // 중복 클릭 원천 방지
            if (popupCanvasGroup != null) popupCanvasGroup.blocksRaycasts = false;
            if (_rerollButton != null) _rerollButton.interactable = false;
            if (_skipButton != null) _skipButton.interactable = false;

            // 루프를 돌며 클릭된 카드와 탈락한 카드를 분리하여 DoTween 시퀀스 극적 연출 집행
            for (int i = 0; i < cardSlots.Count; i++)
            {
                if (cardSlots[i] == null) continue;

                // 인덱스는 1부터 시작하므로 (i + 1) 비교
                if ((i + 1) == slotIndex)
                {
                    // 선택된 카드의 확대 강조 스케일 업 스케일 다운 시퀀스 가동
                    cardSlots[i].PlaySelectedSequence(() =>
                    {
                        // 연출 시퀀스가 완전히 다 끝난 찰나의 순간(OnComplete 콜백)에 실제 레벨을 반영
                        if (!string.IsNullOrEmpty(behaviorID))
                        {
                            AugmentManager.Instance.UpgradeAugment(behaviorID);
                        }

                        if (_pvpSelectionBridge != null)
                        {
                            _pvpSelectionBridge.NotifySelectionCompleted();
                        }

                        // 데이터 반영 후 안전하게 팝업 클로즈
                        ClosePopup();
                    });
                }
                else
                {
                    // 선택받지 못한 카드는 쓸쓸하게 낙하 페이드아웃
                    cardSlots[i].PlayFadeOutSequence();
                }
            }
        }

        private void ClosePopup()
        {
            if (popupCanvasGroup != null)
            {
                // 팝업창 전체가 스르륵 꺼지는 페이드 연출
                popupCanvasGroup.DOFade(0f, 0.2f).SetUpdate(true).OnComplete(() =>
                {
                    gameObject.SetActive(false);
                    Time.timeScale = 1f; // 완벽하게 화면이 닫힌 뒤 인게임 재개
                    IsOpen = false;
                    OnAugmentSelectionCompleted?.Invoke();
                });
            }
            else
            {
                gameObject.SetActive(false);
                Time.timeScale = 1f;
                IsOpen = false;
                OnAugmentSelectionCompleted?.Invoke();
            }
        }
    }
}