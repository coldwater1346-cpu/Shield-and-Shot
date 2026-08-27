using System;

#if UNITY_ANDROID
using System.Text.RegularExpressions;
using BackEnd;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif

namespace Shield_Shot.DataManagement.Login
{
    public class GpgsLoginStrategy : ILoginStrategy
    {
#if UNITY_ANDROID
        private const string FunctionName =
            "GPGSAuthFunction";
#endif

        public void Login(
            LoginRequest request,
            Action<LoginResult> onComplete)
        {
#if UNITY_ANDROID
            LoginOnAndroid(onComplete);
#else
            onComplete?.Invoke(
                LoginResult.Failure(
                    "GPGS_UNSUPPORTED_PLATFORM",
                    "Google Play Games 로그인은 Android에서만 지원됩니다."));
#endif
        }

#if UNITY_ANDROID

        private void LoginOnAndroid(
            Action<LoginResult> onComplete)
        {
            if (onComplete == null)
            {
                return;
            }

            PlayGamesPlatform.Activate();

            PlayGamesPlatform.Instance.Authenticate(status =>
            {
                if (status != SignInStatus.Success)
                {
                    onComplete(
                        LoginResult.Failure(
                            "GPGS_AUTH_FAILED",
                            $"GPGS 인증 실패: {status}"));

                    return;
                }

                RequestAuthCode(onComplete);
            });
        }

        private void RequestAuthCode(
            Action<LoginResult> onComplete)
        {
            PlayGamesPlatform.Instance.RequestServerSideAccess(
                true,
                authCode =>
                {
                    if (string.IsNullOrEmpty(authCode))
                    {
                        onComplete(
                            LoginResult.Failure(
                                "GPGS_AUTH_CODE_EMPTY",
                                "Google 서버 인증 코드를 받지 못했습니다."));

                        return;
                    }

                    EnsureBackendSession(
                        authCode,
                        onComplete);
                });
        }

        private void EnsureBackendSession(
            string authCode,
            Action<LoginResult> onComplete)
        {
            // 서버 펑션 호출에 필요한 뒤끝 로그인 세션 확보
            if (!Backend.IsLogin)
            {
                var guestBro =
                    Backend.BMember.GuestLogin();

                if (!guestBro.IsSuccess())
                {
                    onComplete(
                        LoginResult.Failure(
                            guestBro.GetErrorCode(),
                            $"임시 게스트 인증 실패: " +
                            $"{guestBro.GetMessage()}"));

                    return;
                }
            }

            InvokeGpgsAuthFunction(
                authCode,
                onComplete);
        }

        private void InvokeGpgsAuthFunction(
            string authCode,
            Action<LoginResult> onComplete)
        {
            Param param = new Param();
            param.Add(
                "serverAuthCode",
                authCode);

            Backend.BFunc.InvokeFunction(
                FunctionName,
                param,
                bro =>
                {
                    if (!bro.IsSuccess())
                    {
                        onComplete(
                            LoginResult.Failure(
                                bro.GetErrorCode(),
                                $"GPGS 서버 펑션 호출 실패: " +
                                $"{bro.GetMessage()}"));

                        return;
                    }

                    string rawResponse =
                        bro.GetReturnValue();

                    if (!TryExtractCredentials(
                            rawResponse,
                            out string customId,
                            out string customPw))
                    {
                        onComplete(
                            LoginResult.Failure(
                                "GPGS_RESPONSE_PARSE_FAILED",
                                "서버 인증 응답에서 계정 정보를 확인할 수 없습니다."));

                        return;
                    }

                    LoginOrSignUp(
                        customId,
                        customPw,
                        onComplete);
                });
        }

        private void LoginOrSignUp(
            string customId,
            string customPw,
            Action<LoginResult> onComplete)
        {
            var loginBro =
                Backend.BMember.CustomLogin(
                    customId,
                    customPw);

            if (loginBro.IsSuccess())
            {
                onComplete(
                    LoginResult.Success());

                return;
            }

            var signUpBro =
                Backend.BMember.CustomSignUp(
                    customId,
                    customPw);

            if (signUpBro.IsSuccess())
            {
                onComplete(
                    LoginResult.Success());

                return;
            }

            onComplete(
                LoginResult.Failure(
                    signUpBro.GetErrorCode(),
                    $"GPGS 계정 생성 실패:또는 탈퇴 진행 계정 " +
                    $"{signUpBro.GetMessage()}"));
        }

        private bool TryExtractCredentials(
            string rawResponse,
            out string customId,
            out string customPw)
        {
            customId = null;
            customPw = null;

            if (string.IsNullOrEmpty(rawResponse))
            {
                return false;
            }

            Match idMatch = Regex.Match(
                rawResponse,
                @"gpgs_[a-zA-Z0-9_]+");

            Match pwMatch = Regex.Match(
                rawResponse,
                @"[a-fA-F0-9]{64}");

            if (!idMatch.Success ||
                !pwMatch.Success)
            {
                return false;
            }

            customId = idMatch.Value;
            customPw = pwMatch.Value;

            return true;
        }

#endif
    }
}