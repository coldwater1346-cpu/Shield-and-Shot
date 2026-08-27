using UnityEngine;



namespace Shield_Shot.DataManagement.Login
{
    public class LoginResult
    {
        public bool IsSuccess { get; }
        public string ErrorCode { get; }
        public string Message { get; }

        private LoginResult(
            bool isSuccess,
            string errorCode,
            string message)
        {
            IsSuccess = isSuccess;
            ErrorCode = errorCode;
            Message = message;
        }

        public static LoginResult Success()
        {
            return new LoginResult(true, null, null);
        }

        public static LoginResult Failure(
            string errorCode,
            string message)
        {
            return new LoginResult(false, errorCode, message);
        }
    }
}