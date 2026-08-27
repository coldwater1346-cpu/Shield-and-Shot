using UnityEngine;



namespace Shield_Shot.DataManagement.Login
{
    public interface ILoginStrategy
    {
        void Login(LoginRequest request, System.Action<LoginResult> onComplete);
    }
}