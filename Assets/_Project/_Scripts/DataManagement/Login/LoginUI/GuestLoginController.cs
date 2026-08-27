using BackEnd;
using Shield_Shot.DataManagement;
using Shield_Shot.DataManagement.InventorySystem;
using Shield_Shot.NetworkCore;
using Shield_Shot.NetworkCore.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GuestLoginController : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private Button guestLoginButton;
    [SerializeField] private GameObject loadingPanel; 
    [SerializeField] private ErrorPopupController errorPopup;

    [Header("Sound")]
    [Tooltip("로그인 성공 시 재생할 사운드")]
    [SerializeField] private AudioClip loginSuccessSfx;
    [SerializeField, Range(0f, 1f)] private float loginSuccessSfxVolume = 0.5f;

    private void Start()
    {
        // 버튼이 인스펙터에 연결되어 있다면 코드 상에서 클릭 이벤트 자동 연결
        if (guestLoginButton != null)
        {
            guestLoginButton.onClick.AddListener(OnClickGuestLogin);
        }
    }

    /// <summary>
    /// 게스트 로그인 버튼 클릭 시 호출할 함수 (Button OnClick 이벤트에 연결)
    /// </summary>
    public void OnClickGuestLogin()
    {
        Debug.Log("[GuestLogin] 게스트 로그인 시도...");

        // 1. 게스트 로그인 시도 시작 시 로딩 패널 활성화
        if (loadingPanel != null) loadingPanel.SetActive(true);

        // 뒤끝 비동기 게스트 로그인 호출 (UI 렉 방지)
        Backend.BMember.GuestLogin((bro) =>
        {
            if (bro.IsSuccess())
            {
                Debug.Log($"[GuestLogin] 게스트 로그인 성공 inDate: {Backend.UserInDate}");

                // 서버 데이터 로드 후 씬 이동 (GPGSLoginController와 동일한 로직 호출)
                
                LoadServerDataAndChangeScene();
            }
            else
            {
                string errorCode = bro.GetErrorCode();
                string errorMsg = bro.GetMessage();

                Debug.LogError($"[GuestLogin] 게스트 로그인 실패: {errorCode} / {errorMsg}");

                //  로그인 실패 시 로딩 패널 비활성화 후 에러 팝업 출력
                if (loadingPanel != null) loadingPanel.SetActive(false);

                if (errorPopup != null)
                {
                    errorPopup.ShowError($"게스트 로그인 실패\n[{errorCode}] {errorMsg}");
                }
            }
        });
    }

    private void LoadServerDataAndChangeScene()
    {
        // 1. 로그인 성공 사운드 재생
        if (Shield_Shot.Audio.SoundManager.Instance != null)
        {
            Shield_Shot.Audio.SoundManager.Instance.PlayUI(loginSuccessSfx, loginSuccessSfxVolume);
        }

        // 2. 서버 테이블/차트 데이터 로드
        Shield_Shot.DataManagement.DataParsing.ItemDataParsingManager.Instance.LoadWeaponTableFromServer("248748");
        Shield_Shot.DataManagement.DataParsing.ItemDataParsingManager.Instance.LoadShieldTableFromServer("248748");
        Shield_Shot.DataManagement.DataParsing.ItemDataParsingManager.Instance.LoadEnhanceCostTableFromServer("245979");
        Shield_Shot.DataManagement.DataParsing.ItemDataParsingManager.Instance.LoadItemPriceTableFromServer("246710");
        Shield_Shot.DataManagement.DataParsing.ItemDataParsingManager.Instance.LoadItemCombineTableFromServer("246759");
        Shield_Shot.DataManagement.DataParsing.ItemDataParsingManager.Instance.LoadPropertyRateTableFromServer("246943");

        // 3. 몬스터 및 스테이지 테이블 로드
        Shield_Shot.DataManagement.DataParsing.MonsterDataParsingManager.Instance.LoadMonsterTableFromServer("246477");
        Shield_Shot.DataManagement.DataParsing.StageDataParsingManager.Instance.LoadStageWaveTableFromServer("246469", "246479", "246480", "246481", "246471");

        // 4. 유저 게임 데이터 동기화
        BackendGameData.Instance.GameDataGet();
        PlayerDataManager.Instance.gold = BackendGameData.userData.gold;
        PlayerDataManager.Instance.diamond = BackendGameData.userData.diamond;
        PlayerDataManager.Instance.clearStageStep = BackendGameData.userData.clearStageStep;
        PlayerDataManager.Instance.profileId = BackendGameData.userData.profileId;
        PlayerDataManager.Instance.frameId = BackendGameData.userData.frameId;

        // 5. 인벤토리 데이터 초기화
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.InitInventoryData();
        }
        else
        {
            Debug.LogError("[GuestLogin] InventoryManager.Instance를 찾을 수 없습니다!");
        }

        // 6. 로비 씬 이동
        SceneManager.LoadScene("03_Lobby");
    }
}