namespace eCommerce.Application.Common;

public sealed class Result<T>
{
    private Result(T? value, Error? error, bool isSuccess)
    {
        Value = value;
        Error = error;
        IsSuccess = isSuccess;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public Error? Error { get; }

    public static Result<T> Success(T value) => new(value, null, true);

    public static Result<T> BadRequest(string message, string code = "bad_request") =>
        Failure(message, code, ErrorType.Validation);

    public static Result<T> Validation(string message, IReadOnlyList<string>? errors = null, string code = "validation_error") =>
        new(default, new Error(code, message, ErrorType.Validation, errors), false);

    public static Result<T> NotFound(string message, string code = "not_found") =>
        Failure(message, code, ErrorType.NotFound);

    public static Result<T> Conflict(string message, string code = "conflict") =>
        Failure(message, code, ErrorType.Conflict);

    public static Result<T> Failure(string message, string code = "failure", ErrorType type = ErrorType.Failure) =>
        new(default, new Error(code, message, type), false);
}
