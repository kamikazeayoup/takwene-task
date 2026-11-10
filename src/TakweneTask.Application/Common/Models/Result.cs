namespace TakweneTask.Application.Common.Models;

public class Result
{
    public bool IsSuccess { get; }
    public ErrorType ErrorType { get; }
    public string? ErrorMessage { get; }

    protected Result(bool isSuccess, ErrorType errorType, string? errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorType = errorType;
        ErrorMessage = errorMessage;
    }

    public static Result Success() => new(true, ErrorType.None, null);
    public static Result NotFound(string message) => new(false, ErrorType.NotFound, message);
    public static Result Conflict(string message) => new(false, ErrorType.Conflict, message);
    public static Result Unauthorized(string message) => new(false, ErrorType.Unauthorized, message);
    public static Result Validation(string message) => new(false, ErrorType.Validation, message);
    public static Result Failure(string message) => new(false, ErrorType.Failure, message);
}

public class Result<T> : Result
{
    public T? Value { get; }

    private Result(bool isSuccess, T? value, ErrorType errorType, string? errorMessage)
        : base(isSuccess, errorType, errorMessage)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(true, value, ErrorType.None, null);
    public static new Result<T> NotFound(string message) => new(false, default, ErrorType.NotFound, message);
    public static new Result<T> Conflict(string message) => new(false, default, ErrorType.Conflict, message);
    public static new Result<T> Unauthorized(string message) => new(false, default, ErrorType.Unauthorized, message);
    public static new Result<T> Validation(string message) => new(false, default, ErrorType.Validation, message);
    public static new Result<T> Failure(string message) => new(false, default, ErrorType.Failure, message);
}
