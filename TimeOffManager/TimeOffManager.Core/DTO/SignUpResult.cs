namespace TimeOffManager.Core.DTO
{
    public record SignUpResult
    {
        public string Message { get; init; }
        public bool Success {  get; init; }

        public SignUpResult(string message, bool success)
        {
            Message = message;
            Success = success;
        }

        public static SignUpResult Error(string message) => new SignUpResult(message, false);
        public static SignUpResult Ok() => new SignUpResult("", true);

    }
}
