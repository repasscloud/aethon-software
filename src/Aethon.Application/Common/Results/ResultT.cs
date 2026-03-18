namespace Aethon.Application.Common.Results;

public sealed class Result<T> : Result
{
    public T? Value { get; }

    private Result(bool succeeded, T? value, string? errorCode = null, string? errorMessage = null)
        : base(succeeded, errorCode, errorMessage)
    {
        Value = value;
    }

    public static Result<T> Success(T value)
        => new(true, value);

    public static new Result<T> Failure(string errorCode, string errorMessage)
        => new(false, default, errorCode, errorMessage);
}