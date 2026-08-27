using BackEnd;
using Shield_Shot.DataManagement;
using Shield_Shot.DataManagement.InventorySystem;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Shield_Shot.NetworkCore.UI
{
    public class LoginController : MonoBehaviour
    {
        [Header("Input Fields")]
        [SerializeField] private TMP_InputField idInputField;
        [SerializeField] private TMP_InputField pwInputField;

        [Header("Submit Button")]
        [SerializeField] private Button loginSubmitButton;

        [Header("UI 연결")]
        [SerializeField] private GameObject loadingPanel; 

        [Header("Error Popup Reference")]
        [SerializeField] private ErrorPopupController errorPopup;

        [Header("Sound")]
        [Tooltip("로그인 성공 시 재생할 사운드")]
        [SerializeField] private AudioClip loginSuccessSfx;
        [SerializeField, Range(0f, 1f)] private float loginSuccessSfxVolume = 0.5f;

        private void Awake()
        {
            if (errorPopup == null)
            {
                errorPopup = GetComponentInParent<Canvas>().GetComponentInChildren<ErrorPopupController>(true);
            }
            if (loginSubmitButton != null)
            {
                loginSubmitButton.onClick.AddListener(OnClickLoginSubmit);
            }
        }

        private void OnDestroy()
        {
            if (loginSubmitButton != null)
                loginSubmitButton.onClick.RemoveListener(OnClickLoginSubmit);
        }

        private void OnClickLoginSubmit()
        {
            string id = idInputField.text;
            string pw = pwInputField.text;

            // 1. 입력 필드 검증 (실패 시 로딩 패널을 키지 않고 바로 예외 출력)
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(pw))
            {
                errorPopup.ShowError("Please fill in all fields.");
                return;
            }

            // 2. 뒤끝 초기화 상태 검증
            if (!Backend.IsInitialized)
            {
                errorPopup.ShowError("서버 연결 중입니다. 2~3초 뒤 다시 시도해주세요.");
                return;
            }

            Debug.Log("커스텀 로그인 요청 중...");

            // 3. 실제 로그인 통신 시작 직전 로딩 패널 활성화
            if (loadingPanel != null) loadingPanel.SetActive(true);

            // Backend.CustomLogin 호출 후 진짜 에러 결과 받아오기
            var bro = Backend.BMember.CustomLogin(id, pw);

            if (bro.IsSuccess())
            {
                // 성공 시 씬이 전환되므로 loadingPanel은 그대로 둡니다.
                LoadServerDataAndChangeScene();
            }
            else
            {
                //  로그인 실패 시 로딩 패널 비활성화 후 에러 팝업
                if (loadingPanel != null) loadingPanel.SetActive(false);

                string errorMsg =
                    $"로그인 실패\n" +
                    $"StatusCode : {bro.GetStatusCode()}\n" +
                    $"ErrorCode : {bro.GetErrorCode()}\n" +
                    $"Message : {bro.GetMessage()}";

                Debug.LogError(errorMsg);
                errorPopup.ShowError(errorMsg);
            }
        }

        /// <summary>
        /// 로그인 성공 후 서버 데이터 로드 및 씬 이동 (나중에 서버 데이터 로더로 분리될 영역)
        /// </summary>
        private void LoadServerDataAndChangeScene()
        {
            if (Shield_Shot.Audio.SoundManager.Instance != null)
                Shield_Shot.Audio.SoundManager.Instance.PlayUI(loginSuccessSfx, loginSuccessSfxVolume);

            // 차트 및 유저 데이터 로드
            Shield_Shot.DataManagement.DataParsing.ItemDataParsingManager.Instance.LoadWeaponTableFromServer("248748");
            Shield_Shot.DataManagement.DataParsing.ItemDataParsingManager.Instance.LoadShieldTableFromServer("248748");
            Shield_Shot.DataManagement.DataParsing.ItemDataParsingManager.Instance.LoadEnhanceCostTableFromServer("245979");
            Shield_Shot.DataManagement.DataParsing.ItemDataParsingManager.Instance.LoadItemPriceTableFromServer("246710");
            Shield_Shot.DataManagement.DataParsing.ItemDataParsingManager.Instance.LoadItemCombineTableFromServer("246759");
            Shield_Shot.DataManagement.DataParsing.ItemDataParsingManager.Instance.LoadPropertyRateTableFromServer("246943");

            Shield_Shot.DataManagement.DataParsing.MonsterDataParsingManager.Instance.LoadMonsterTableFromServer("246477");
            Shield_Shot.DataManagement.DataParsing.StageDataParsingManager.Instance.LoadStageWaveTableFromServer("246469", "246479", "246480", "246481", "246471");

            BackendGameData.Instance.GameDataGet();
            PlayerDataManager.Instance.gold = BackendGameData.userData.gold;
            PlayerDataManager.Instance.diamond = BackendGameData.userData.diamond;
            PlayerDataManager.Instance.clearStageStep = BackendGameData.userData.clearStageStep;
            PlayerDataManager.Instance.profileId = BackendGameData.userData.profileId;
            PlayerDataManager.Instance.frameId = BackendGameData.userData.frameId;

            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.InitInventoryData();
            }
            else
            {
                Debug.LogError("[Login] InventoryManager.Instance를 찾을 수 없습니다!");
            }

            SceneManager.LoadScene("03_Lobby");
        }
    }
}