using BackEnd;
using Shield_Shot.DataManagement.Login;
using Shield_Shot.NetworkCore.UI;
using UnityEngine;

namespace Shield_Shot.DataManagement.Login
{

    public class LoginCoordinator : MonoBehaviour
    {
        [SerializeField] private GameObject loadingPanel;
        [SerializeField] private ErrorPopupController errorPopup;
        [SerializeField] private PostLoginInitializer postLoginInitializer;

        private bool _isLoggingIn;


        private void Awake()
        {
            if (errorPopup == null)
            {
                errorPopup =
                    FindFirstObjectByType<ErrorPopupController>();
            }

            if (postLoginInitializer == null)
            {
                postLoginInitializer =
                    GetComponent<PostLoginInitializer>();
            }
        }
        public void Execute(
            ILoginStrategy strategy,
            LoginRequest request)
        {
            if (_isLoggingIn)
                return;

            if (!Backend.IsInitialized)
            {
                errorPopup.ShowError("서버 연결 중입니다.");
                return;
            }

            _isLoggingIn = true;
            SetLoading(true);

            strategy.Login(request, result =>
            {
                if (!result.IsSuccess)
                {
                    _isLoggingIn = false;
                    SetLoading(false);
                    errorPopup.ShowError(result.Message);
                    return;
                }

                postLoginInitializer.Initialize(result =>
                {
                    if (!result.IsSuccess)
                    {
                        _isLoggingIn = false;
                        SetLoading(false);
                        errorPopup.ShowError(result.Message);
                    }
                });
            });
        }

        private void SetLoading(bool active)
        {
            if (loadingPanel != null)
                loadingPanel.SetActive(active);
        }
    }
}