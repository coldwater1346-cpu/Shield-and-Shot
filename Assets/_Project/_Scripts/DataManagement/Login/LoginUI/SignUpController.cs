using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Shield_Shot.UI;
using System.Text.RegularExpressions; // 정규식(Regex) 사용

namespace Shield_Shot.NetworkCore.UI
{
    public class SignUpController : MonoBehaviour
    {
        [SerializeField] private LoginTabController _loginTabController;

        [Header("Input Fields")]
        [SerializeField] private TMP_InputField idInputField;
        [SerializeField] private TMP_InputField pwInputField;
        [SerializeField] private TMP_InputField pwConfirmInputField;

        [Header("Submit Button")]
        [SerializeField] private Button signUpSubmitButton;

        [Header("Nickname Popup System")]
        [SerializeField] private GameObject nicknamePopupPanel;
        [SerializeField] private TMP_InputField nicknameInputField;
        [SerializeField] private Button nicknameSubmitButton;

        [Header("Error Popup Reference")]
        [SerializeField] private ErrorPopupController errorPopup;

        [Header("ID Rule Check UI")]
        [SerializeField] private Button idCheckButton;   // ID 규칙 검사 버튼
        [SerializeField] private TMP_Text idStatusText;   // 결과를 보여줄 텍스트

        [Header("PW Rule Check UI")]
        [SerializeField] private Button pwCheckButton;   // PW 규칙 검사 버튼
        [SerializeField] private TMP_Text pwStatusText;   // 결과를 보여줄 텍스트

        // 규칙 검증 여부 
        private bool _isIdRulePassed = false;
        private bool _isPwRulePassed = false;

        private void Awake()
        {
            if (errorPopup == null)
            {
                errorPopup = GetComponentInParent<Canvas>().GetComponentInChildren<ErrorPopupController>(true);
            }

            if (_loginTabController == null)
            {
                _loginTabController = FindFirstObjectByType<LoginTabController>();
            }

            // 버튼 리스너 등록
            if (signUpSubmitButton != null)
                signUpSubmitButton.onClick.AddListener(OnClickSignUpSubmit);

            if (nicknameSubmitButton != null)
                nicknameSubmitButton.onClick.AddListener(OnClickNicknameSubmit);

            if (idCheckButton != null)
                idCheckButton.onClick.AddListener(OnClickIdRuleCheck);

            if (pwCheckButton != null)
                pwCheckButton.onClick.AddListener(OnClickPwRuleCheck);

            // 입력 필드 실시간 감시 및 초기화 세팅
            InitStatusTexts();
        }

        private void OnDestroy()
        {
            if (signUpSubmitButton != null)
                signUpSubmitButton.onClick.RemoveListener(OnClickSignUpSubmit);

            if (nicknameSubmitButton != null)
                nicknameSubmitButton.onClick.RemoveListener(OnClickNicknameSubmit);

            if (idCheckButton != null)
                idCheckButton.onClick.RemoveListener(OnClickIdRuleCheck);

            if (pwCheckButton != null)
                pwCheckButton.onClick.RemoveListener(OnClickPwRuleCheck);
        }

        /// <summary>
        /// 상태 텍스트 초기화 및 입력창 실시간 상태 감시 리스너 등록
        /// </summary>
        private void InitStatusTexts()
        {
            // ID 입력창 실시간 감시
            if (idInputField != null)
            {
                idInputField.onValueChanged.AddListener((text) =>
                {
                    _isIdRulePassed = false;
                    // 만약 입력 글자를 다 지워서 비어있게 되면 기본 설명(검은색)으로 복구
                    if (string.IsNullOrEmpty(text))
                    {
                        SetStatusText(idStatusText, "최소 4자 이상, 특수문자/공백 제외", Color.black);
                    }
                });
            }

            // PW 입력창 실시간 감시
            if (pwInputField != null)
            {
                pwInputField.onValueChanged.AddListener((text) =>
                {
                    _isPwRulePassed = false;
                    // 만약 입력 글자를 다 지워서 비어있게 되면 기본 설명(검은색)으로 복구
                    if (string.IsNullOrEmpty(text))
                    {
                        SetStatusText(pwStatusText, "영문 + 숫자 + 특수문자 조합 필수", Color.black);
                    }
                });
            }

            // 시작할 때 기본 안내 텍스트 출력 (검은색)
            SetStatusText(idStatusText, "최소 4자 이상, 특수문자/공백 제외", Color.black);
            SetStatusText(pwStatusText, "영문 + 숫자 + 특수문자 조합 필수", Color.black);
        }

        /// <summary>
        /// 회원 가입 버튼 클릭 시 호출
        /// </summary>
        private void OnClickSignUpSubmit()
        {
            string id = idInputField.text.Trim();
            string pw = pwInputField.text;
            string pwConfirm = pwConfirmInputField.text;

            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(pw) || string.IsNullOrEmpty(pwConfirm))
            {
                errorPopup.ShowError("Please fill in all fields.");
                return;
            }

            if (!_isIdRulePassed || !_isPwRulePassed)
            {
                errorPopup.ShowError("ID 및 PW 규칙 검사를 완료해주세요.");
                return;
            }

            if (pw != pwConfirm)
            {
                errorPopup.ShowError("Passwords do not match.");
                return;
            }

            bool isSignUpSuccess = BackendLogin.Instance.CustomSignUp(id, pw);

            if (isSignUpSuccess)
            {
                bool isLoginSuccess = BackendLogin.Instance.CustomLogin(id, pw);
                if (isLoginSuccess)
                {
                    if (nicknamePopupPanel != null) nicknamePopupPanel.SetActive(true);
                }
            }
            else
            {
                SetStatusText(idStatusText, "이미 존재하는 아이디입니다.", Color.red);
                errorPopup.ShowError("This ID is already taken.");
            }
        }

        /// <summary>
        /// ID 규칙 검사 버튼 클릭 시 호출
        /// </summary>
        private void OnClickIdRuleCheck()
        {
            string id = idInputField.text.Trim();

            if (string.IsNullOrEmpty(id))
            {
                SetStatusText(idStatusText, "아이디를 입력해주세요.", Color.red);
                _isIdRulePassed = false;
                return;
            }

            if (id.Length < 4)
            {
                SetStatusText(idStatusText, "아이디는 최소 4자 이상이어야 합니다.", Color.red);
                _isIdRulePassed = false;
                return;
            }

            if (!Regex.IsMatch(id, "^[a-zA-Z0-9]+$"))
            {
                SetStatusText(idStatusText, "특수문자나 공백은 포함할 수 없습니다.", Color.red);
                _isIdRulePassed = false;
                return;
            }

            SetStatusText(idStatusText, "사용 가능한 아이디 양식입니다!", Color.green);
            _isIdRulePassed = true;
        }

        /// <summary>
        /// PW 규칙 검사 버튼 클릭 시 호출
        /// </summary>
        private void OnClickPwRuleCheck()
        {
            string pw = pwInputField.text;

            if (string.IsNullOrEmpty(pw))
            {
                SetStatusText(pwStatusText, "비밀번호를 입력해주세요.", Color.red);
                _isPwRulePassed = false;
                return;
            }

            if (pw.Contains(" "))
            {
                SetStatusText(pwStatusText, "비밀번호에 공백을 사용할 수 없습니다.", Color.red);
                _isPwRulePassed = false;
                return;
            }

            string pwPattern = @"^(?=.*[a-zA-Z])(?=.*\d)(?=.*[^a-zA-Z0-9\s]).+$";

            if (!Regex.IsMatch(pw, pwPattern))
            {
                SetStatusText(pwStatusText, "영문, 숫자, 특수문자를 최소 1개씩 포함해야 합니다.", Color.red);
                _isPwRulePassed = false;
                return;
            }

            SetStatusText(pwStatusText, "안전한 비밀번호 양식입니다!", Color.green);
            _isPwRulePassed = true;
        }

        private void SetStatusText(TMP_Text targetText, string message, Color color)
        {
            if (targetText != null)
            {
                targetText.text = message;
                targetText.color = color;
            }
        }
        //닉네임 확인
        private void OnClickNicknameSubmit()
        {
            string nickname = nicknameInputField.text.Trim();

            if (string.IsNullOrEmpty(nickname))
            {
                errorPopup.ShowError("Please fill in all fields.");
                return;
            }

            bool isNickSuccess =
                BackendLogin.Instance.UpdateNickname(nickname);

            if (!isNickSuccess)
            {
                errorPopup.ShowError(
                    "This nickname is already taken or invalid."
                );

                nicknameInputField.Select();
                nicknameInputField.ActivateInputField();
                return;
            }

            ClearAllInputs();

            if (nicknamePopupPanel != null)
            {
                nicknamePopupPanel.SetActive(false);
            }

            _loginTabController.ShowLoginPanel();
        }

        private void ClearAllInputs()
        {
            idInputField.text = string.Empty;
            pwInputField.text = string.Empty;
            pwConfirmInputField.text = string.Empty;
            nicknameInputField.text = string.Empty;

            _isIdRulePassed = false;
            _isPwRulePassed = false;

            InitStatusTexts();
        }
    }
}