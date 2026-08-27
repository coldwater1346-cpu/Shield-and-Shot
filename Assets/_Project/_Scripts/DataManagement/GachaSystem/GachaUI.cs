using Shield_Shot.DataManagement.GachaSystem;
using Shield_Shot.NetworkCore;
using Shield_Shot.DataManagement; 
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shield_Shot.DataManagement.GachaSystem
{
    public class GachaUI : MonoBehaviour
    {
        [Header("Gacha Controller")]
        [SerializeField] private GachaController _gachaController;

        [Header("Test Buttons")]
        [SerializeField] private Button singleGachaButton;
        [SerializeField] private Button multiGachaButton;
        [SerializeField] private Button thousandGachaButton;

        [Header("Confrim Popups")]
        [SerializeField] private GameObject confirmPopup;
        [SerializeField] private Button confirmYesButton;
        [SerializeField] private Button confirmNoButton;
        [SerializeField] private TextMeshProUGUI confirmText;

        [Header("LackDia Popups")]
        [SerializeField] private GameObject noDiamondPopup;
        [SerializeField] private Button CloseButton;

        [Header("Result Popups")]
        [SerializeField] private GachaResultPopupUI resultPopup;

        private int pendingGachaCount = 0;


        // 가챠 1회당 소모되는 다이아 개수
        [Header("Gacha Cost Settings")]
        [SerializeField] private int singleGachaCost = 10;
        [SerializeField] private int multiGachaCost = 100;
        [SerializeField] private int thousandGachaCost = 1000;

        private void Start()
        {
            if (_gachaController == null)
            {
                _gachaController = FindFirstObjectByType<GachaController>();
            }

            if (confirmPopup != null)
                confirmPopup.SetActive(false);

            if (noDiamondPopup != null)
                noDiamondPopup.SetActive(false);

            if (singleGachaButton != null)
                singleGachaButton.onClick.AddListener(() => OnClickGachaButton(1));

            if (multiGachaButton != null)
                multiGachaButton.onClick.AddListener(() => OnClickGachaButton(10));

            if (confirmYesButton != null)
                confirmYesButton.onClick.AddListener(OnConfirmGachaSuccess);

            if (confirmNoButton != null)
                confirmNoButton.onClick.AddListener(CloseConfirmPopup);

            if (CloseButton != null)
                CloseButton.onClick.AddListener(CloseLackDiaPopup);

            if (thousandGachaButton != null)
                thousandGachaButton.onClick.AddListener(() => OnClickGachaButton(100)); 
        }

        /// <summary>
        /// 1단계: 가챠 버튼 클릭 시 다이아 잔액 비교 후 팝업 분기
        /// </summary>
        private void OnClickGachaButton(int count)
        {
            pendingGachaCount = count;

            // 1회 vs 10회 vs 100회 비용 계산 (thousandGachaCost는 변수명 유지하되 기획값 매칭)
            int requiredCost = (count == 1) ? singleGachaCost : (count == 10) ? multiGachaCost : thousandGachaCost;

            //] 플레이어의 현재 보유 다이아가 필요한 양보다 적은지 검증(실제 검증은 서버에서)
            int currentDiamond = PlayerDataManager.Instance.diamond;

            if (currentDiamond < requiredCost)
            {
                Debug.LogWarning($"[Gacha UI] 다이아 부족! 보유량: {currentDiamond}, 필요량: {requiredCost}");

                // 다이아 부족 팝업 활성화 후 리턴 (컨펌 창 안 열어줌)
                if (noDiamondPopup != null)
                {
                    noDiamondPopup.SetActive(true);
                }
                return;
            }

            Debug.Log($"{count}회 소환 시도 컨펌 팝업 활성화 - 필요 다이아: {requiredCost}");

            // 다이아가 충분하다면 정상적으로 컨펌 창 문구 작성 및 오픈
            if (confirmText != null)
            {
                confirmText.text = string.Format("정말로 {0}개 다이아를 소모하여 {1}회 뽑기를 진행 하시겠습니까?", requiredCost, count);
            }

            if (confirmPopup != null)
            {
                confirmPopup.SetActive(true);
            }
        }

        /// <summary>
        /// 컴펌 팝업 확인 시 GachaController에게 가챠 요청
        /// </summary>
        private void OnConfirmGachaSuccess()
        {
            CloseConfirmPopup();

            if (_gachaController == null)
            {
                Debug.LogError("GachaController가 연결되어 있지 않습니다.");
                return;
            }

            _gachaController.RequestGachaResultFromServer(pendingGachaCount);
        }

        private void StartCardPresentation()
        {
            Debug.Log("카드 연출");
        }

        private void CloseConfirmPopup()
        {
            if (confirmPopup != null)
                confirmPopup.SetActive(false);
        }

        private void CloseLackDiaPopup()
        {
            if (noDiamondPopup != null)
            {
                noDiamondPopup.SetActive(false);
            }
        }
    }
}