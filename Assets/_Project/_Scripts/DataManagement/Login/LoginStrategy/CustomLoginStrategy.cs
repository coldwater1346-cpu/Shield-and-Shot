using BackEnd;
using Shield_Shot.DataManagement.Login;
using UnityEngine;

namespace Shield_Shot.DataManagement.Login
{
    public class CustomLoginStrategy : ILoginStrategy
    {
        public void Login(LoginRequest request, System.Action<LoginResult> onComplete)
        {
            var bro = Backend.BMember.CustomLogin(
                request.id,
                request.password);

            if (bro.IsSuccess())
            {
                onComplete(LoginResult.Success());
                return;
            }

            onComplete(LoginResult.Failure(
                bro.GetErrorCode(),
                bro.GetMessage()));
        }
    }
}