using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Shield_Shot.NetworkCore.UI;

namespace Shield_Shot.DataManagement.Login
{
    public class LoginUIController : MonoBehaviour
    {
        [Header("Login Coordinator")]
        [SerializeField]
        private LoginCoordinator coordinator;

        [Header("Custom Login Input Fields")]
        [SerializeField] private TMP_InputField idInputField;

        [SerializeField] private TMP_InputField pwInputField;

        [Header("Login Buttons")]
        [SerializeField] private Button customLoginButton;
        [SerializeField] private Button guestLoginButton;
        [SerializeField] private Button gpgsLoginButton;

        [Header("Error Popup")]
        [SerializeField] private ErrorPopupController errorPopup;

        private void Awake()
        {
            FindMissingReferences();
            RegisterButtonEvents();
        }

        private void OnDestroy()
        {
            UnregisterButtonEvents();
        }

        private void FindMissingReferences()
        {
            if (coordinator == null)
            {
                coordinator = FindFirstObjectByType<LoginCoordinator>();
            }

            if (errorPopup == null)
            {
                errorPopup =  FindFirstObjectByType<ErrorPopupController>();
            }
        }

        private void RegisterButtonEvents()
        {
            if (customLoginButton != null)
            {
                customLoginButton.onClick.AddListener( OnClickCustomLogin);
            }

            if (guestLoginButton != null)
            {
                guestLoginButton.onClick.AddListener( OnClickGuestLogin);
            }

            if (gpgsLoginButton != null)
            {
                gpgsLoginButton.onClick.AddListener( OnClickGpgsLogin);
            }
        }

        private void UnregisterButtonEvents()
        {
            if (customLoginButton != null)
            {
                customLoginButton.onClick.RemoveListener( OnClickCustomLogin);
            }

            if (guestLoginButton != null)
            {
                guestLoginButton.onClick.RemoveListener( OnClickGuestLogin);
            }

            if (gpgsLoginButton != null)
            {
                gpgsLoginButton.onClick.RemoveListener( OnClickGpgsLogin);
            }
        }

        public void OnClickCustomLogin()
        {
            if (coordinator == null)
            {
                ShowError("로그인 시스템을 찾을 수 없습니다.");

                return;
            }

            if (idInputField == null ||
                pwInputField == null)
            {
                ShowError( "로그인 입력 필드가 연결되지 않았습니다.");

                return;
            }

            string id = idInputField.text.Trim();
            string password = pwInputField.text;

            if (string.IsNullOrEmpty(id) ||
                string.IsNullOrEmpty(password))
            {
                ShowError( "Please fill in all fields.");

                return;
            }

            coordinator.Execute(
                new CustomLoginStrategy(),
                new LoginRequest
                {
                    id = id,
                    password = password
                });
        }

        public void OnClickGuestLogin()
        {
            if (coordinator == null)
            {
                ShowError( "로그인 시스템을 찾을 수 없습니다.");

                return;
            }

            coordinator.Execute(
                new GuestLoginStrategy(),
                new LoginRequest());
        }

        public void OnClickGpgsLogin()
        {
            if (coordinator == null)
            {
                ShowError( "로그인 시스템을 찾을 수 없습니다.");

                return;
            }

            coordinator.Execute(new GpgsLoginStrategy(), new LoginRequest());
        }

        private void ShowError(string message)
        {
            if (errorPopup != null)
            {
                errorPopup.ShowError(message);
            }
            else
            {
                Debug.LogError($"[LoginUIController] {message}");
            }
        }
    }
}