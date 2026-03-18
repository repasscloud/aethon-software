namespace Aethon.Application.Common.Results;

public class Result
{
    public bool Succeeded { get; }
    public string? ErrorCode { get; }
    public string? ErrorMessage { get; }

    protected Result(bool succeeded, string? errorCode = null, string? errorMessage = null)
    {
        Succeeded = succeeded;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public static Result Success() => new(true);

    public static Result Failure(string errorCode, string errorMessage)
        => new(false, errorCode, errorMessage);
}