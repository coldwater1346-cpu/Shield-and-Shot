namespace Shield_Shot.DataManagement.Login
{
    public sealed class InitializationResult
    {
        public bool IsSuccess { get; }
        public string Message { get; }

        private InitializationResult(bool isSuccess, string message)
        {
            IsSuccess = isSuccess;
            Message = message;
        }

        public static InitializationResult Success()
        {
            return new InitializationResult(true, null);
        }

        public static InitializationResult Failure(string message)
        {
            return new InitializationResult(false, message);
        }
    }
}
