using BackEnd;
using Shield_Shot.DataManagement.Login;
using UnityEngine;

namespace Shield_Shot.DataManagement.Login
{
    public class GuestLoginStrategy : ILoginStrategy
    {
        public void Login(LoginRequest request, System.Action<LoginResult> onComplete)
        {
            Backend.BMember.GuestLogin(bro =>
            {
                if (bro.IsSuccess())
                {
                    onComplete(LoginResult.Success());
                    return;
                }

                onComplete(LoginResult.Failure(
                    bro.GetErrorCode(),
                    bro.GetMessage()));
            });
        }
    }
}