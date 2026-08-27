#if UNITY_ANDROID
using System.Collections;
using Shield_Shot.DataManagement;
using Shield_Shot.DataManagement.InventorySystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using GooglePlayGames;
using GooglePlayGames.BasicApi;

using BackEnd;
using LitJson;
using System.Text.RegularExpressions;

namespace Shield_Shot.NetworkCore.UI
{
    public class GPGSLoginController : MonoBehaviour
    {
        [Header("GPGS Submit Button")]
        [SerializeField] private Button gpgsLoginButton;

        [Header(" Popup Reference")]
        [SerializeField] private GameObject loadingPanel;
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

            if (gpgsLoginButton != null)
            {
                gpgsLoginButton.onClick.AddListener(OnClickGPGSLogin);
            }
        }

        private void OnDestroy()
        {
            if (gpgsLoginButton != null)
            {
                gpgsLoginButton.onClick.RemoveListener(OnClickGPGSLogin);
            }
        }

        /// <summary>
        /// 구글 로그인 버튼 클릭 이벤트
        /// </summary>
        public void OnClickGPGSLogin()
        {
            Debug.Log("[GPGSLogin] 구글 인증 시도 중...");

            // 1. 구글 인증 시도 시작 시 로딩 패널 활성화
            if (loadingPanel != null) loadingPanel.SetActive(true);

            PlayGamesPlatform.Activate();

            PlayGamesPlatform.Instance.Authenticate(status =>
            {
                Debug.Log($"[GPGSLogin] Authenticate 결과 status: {status}");

                if (status == SignInStatus.Success)
                {
                    // [핵심] 최신 GPGS API 규칙:
                    PlayGamesPlatform.Instance.RequestServerSideAccess(true, (authCode) =>
                    {
                        if (string.IsNullOrEmpty(authCode))
                        {
                            Debug.LogError("[GPGSLogin] Auth Code가 null이거나 비어있습니다.");

                            //  Auth Code 획득 실패 시 로딩 패널 비활성화 후 에러 팝업
                            if (loadingPanel != null) loadingPanel.SetActive(false);
                            if (errorPopup != null) errorPopup.ShowError("GPGS Error: Auth Code is null");
                            return;
                        }

                        Debug.Log($"[GPGSLogin] Auth Code 획득 성공! : {authCode}");

                        //  CallBGPGSAuthFunction() 내부에서 성공/실패 시 로딩 패널을 제어하므로 
                        // 여기서는 활성화 상태 유지 후 다음 단계로 넘깁니다.
                        CallBGPGSAuthFunction(authCode);
                    });
                }
                else
                {
                    Debug.LogError($"[GPGSLogin] 구글 인증 실패: {status}");

                    //  구글 인증 실패 시 로딩 패널 비활성화 후 에러 팝업
                    if (loadingPanel != null) loadingPanel.SetActive(false);
                    if (errorPopup != null)
                    {
                        errorPopup.ShowError($"GPGS Failed ({status})\nCheck Tester & WebClientID");
                    }
                }
            });
        }
        /// <summary>
        /// Backend.BFunc.InvokeFunction을 이용한 서버 펑션 호출
        /// </summary>
        private void CallBGPGSAuthFunction(string authCode)
        {
            // 1. 임시 게스트 로그인 세션 가드
            if (!Backend.IsLogin)
            {
                Debug.Log("[GPGSLogin] 임시 뒤끝 세션(게스트) 생성 중...");
                var guestBro = Backend.BMember.GuestLogin();

                if (!guestBro.IsSuccess())
                {
                    if (loadingPanel != null)
                    {
                        loadingPanel.SetActive(false);
                    }

                    if (errorPopup != null) errorPopup.ShowError($"Guest Auth Failed: {guestBro.GetErrorCode()}");
                    return;
                }
            }

            Param param = new Param();
            param.Add("serverAuthCode", authCode);

            Debug.Log("[GPGSLogin] Backend.BFunc.InvokeFunction(GPGSAuthFunction) 호출...");

            Backend.BFunc.InvokeFunction("GPGSAuthFunction", param, (bro) =>
            {
                if (bro.IsSuccess())
                {
                    string rawResponse = bro.GetReturnValue();
                    Debug.Log($"[GPGSLogin] Server Raw: {rawResponse}");

                    try
                    {
                        //  정규식으로 customId (gpgs_로 시작하는 문자열)와 customPw (SHA256 64자리) 직접 추출!
                        var idMatch = Regex.Match(rawResponse, @"gpgs_[a-zA-Z0-9_]+");
                        var pwMatch = Regex.Match(rawResponse, @"[a-fA-F0-9]{64}");

                        if (idMatch.Success && pwMatch.Success)
                        {
                            string customId = idMatch.Value;
                            string customPw = pwMatch.Value;

                            Debug.Log($"[GPGSLogin] ?? 정규식 추출 성공! customId: {customId}");

                            // 1. 뒤끝 로그인 시도
                            var loginBro = Backend.BMember.CustomLogin(customId, customPw);
                            if (loginBro.IsSuccess())
                            {
                                Debug.Log("[GPGSLogin]  기존 계정 구글 로그인 성공!");
                                LoadServerDataAndChangeScene(); //  로비로 이동!
                            }
                            else
                            {
                                // 2. 신규 회원가입 시도
                                var signUpBro = Backend.BMember.CustomSignUp(customId, customPw);
                                if (signUpBro.IsSuccess())
                                {
                                    Debug.Log("[GPGSLogin]  신규 계정 구글 회원가입 성공!");
                                    LoadServerDataAndChangeScene(); //  로비로 이동!
                                }
                                else
                                {
                                    if (loadingPanel != null)
                                    {
                                        loadingPanel.SetActive(false);
                                    }
                                    if (errorPopup != null)
                                        errorPopup.ShowError($"SignUp Error: {signUpBro.GetErrorCode()}\n{signUpBro.GetMessage()}");
                                }
                            }
                        }
                        else
                        {
                            if (loadingPanel != null) loadingPanel.SetActive(false);
                            Debug.LogError($"[GPGSLogin] ID/PW 추출 실패: {rawResponse}");
                            if (errorPopup != null) errorPopup.ShowError($"Regex Parsing Failed:\n{rawResponse}");
                        }
                    }
                    catch (System.Exception ex)
                    {
                        if (loadingPanel != null) loadingPanel.SetActive(false);
                        Debug.LogError($"[GPGSLogin] 예외 발생: {ex.Message}");
                        if (errorPopup != null) errorPopup.ShowError($"Error: {ex.Message}");
                    }
                }
                else
                {
                    if (loadingPanel != null) loadingPanel.SetActive(false);
                    string errorDetail = $"Status: {bro.GetStatusCode()}\nCode: {bro.GetErrorCode()}\nMsg: {bro.GetMessage()}";
                    if (errorPopup != null) errorPopup.ShowError($"BFunc Invoke Failed:\n{errorDetail}");
                }
            });
        }

        // 키값 단순 추출용 도우미 함수
        private string ExtractSimpleValue(string text, string key)
        {
            try
            {
                string search = $"\"{key}\":\"";
                int start = text.IndexOf(search);
                if (start == -1)
                {
                    search = $"\"{key}\": \"";
                    start = text.IndexOf(search);
                }
                if (start == -1) return null;

                start += search.Length;
                int end = text.IndexOf("\"", start);
                return text.Substring(start, end - start);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 데이터 로드 및 씬 이동 (서버 세션이 활성화된 상태에서 데이터 가져옴)
        /// </summary>
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
                Debug.LogError("[GPGSLogin] InventoryManager.Instance를 찾을 수 없습니다!");
            }

            // 6. 로비 씬 이동
            SceneManager.LoadScene("03_Lobby");
        }
    }
}
#endif