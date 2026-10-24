namespace CoreApiRest.BusinessRules;

public static class Result
{
    public static Result<T> Success<T>(T value)
        where T : class
        => Result<T>.Success(value);

    public static Result<T> Failure<T>(Error error)
        where T : class
        => Result<T>.Failure(error);
}

public sealed class Result<T>
    where T : class
{
    private Result(bool isSuccess, T? value, Error? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public T? Value { get; }

    public Error? Error { get; }

    public static Result<T> Success(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new Result<T>(true, value, null);
    }

    public static Result<T> Failure(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new Result<T>(false, null, error);
    }
}
