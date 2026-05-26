using System.Diagnostics.CodeAnalysis;

namespace Shared.Domain.DTOs;

public class Result
{
    public static Result<T> Warn<T>(string message) => new(message);
    public static Result<T> Fail<T>(string message) => new(message);
    public static Result<T> FromException<T>(Exception exception) => new(exception);
    public static Result<T> Ok<T>(T data) => new(data);
}

public class Result<T>
{
    [MemberNotNullWhen(true, nameof(Data))]
    public bool Ok => !(string.IsNullOrEmpty(Message) && _exception == null);
    public T? Data { get; private set; }
    public string Message { get; private set; }
    private readonly Exception? _exception;
    public Exception Exception => _exception ?? throw new InvalidOperationException();

    private Result(string message, Exception? exception)
    {
        Message = message;
        _exception = exception;
    }

    public Result(string message) => Message = message;

    public Result(Exception exception)
    {
        Message = string.Empty;
        _exception = exception;
    }

    public Result(T data)
    {
        Message = string.Empty;
        Data = data;
    }
}

public static class ResultExtensions
{
    public static Result<T> ToResult<T>(this string message) => new Result<T>(message);
    public static Result<T> ToResult<T>(this Exception exception) => new Result<T>(exception);
}