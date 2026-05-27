using System.Diagnostics.CodeAnalysis;

namespace Shared.Domain.DTOs;

public class Result
{
    public static Result<T> Ok<T>(string message) => new(message, type: ResultType.Message);
    public static Result<T> Fail<T>(string message) => new(message, type: ResultType.Error);
    public static Result<T> FromException<T>(Exception exception) => new(exception);
    public static Result<T> Ok<T>(T data) => new(data);
}

public enum ResultType
{
    Data,
    Error,
    Message,
    Exception
}

public class Result<T>
{
    private ResultType Type { get; }

    public T? Data { get; private set; }
    public string? Message { get; private set; }
    
    public Exception? Exception { get; private set; }
    
    
    private Result(string? message, Exception? exception, ResultType type)
    {
        Message = message;
        Exception = exception;
        Type = type;
    }

    public Result(string message, ResultType type = ResultType.Error)
    {
        Message = message;
        Exception = null;
        Type = type;
    }

    public Result(Exception exception)
    {
        Message = string.Empty;
        Exception = exception;
        Type = ResultType.Exception;
    }

    public Result(T data)
    {
        Message = string.Empty;
        Data = data;
        Exception = null;
        Type = ResultType.Data;
    }
    
    [MemberNotNullWhen(true, nameof(Data))]
    public bool HasData => (Data != null && Type == ResultType.Data);
    
    [MemberNotNullWhen(true, nameof(Message))]
    public bool HasMessage => (!string.IsNullOrEmpty(Message) && Type == ResultType.Message);
    
    [MemberNotNullWhen(true, nameof(Message))]
    public bool ErrorRaised => (!string.IsNullOrEmpty(Message) && Type == ResultType.Error);

    [MemberNotNullWhen(true, nameof(Exception))]
    public bool ExceptionThrown => (Exception != null && Type == ResultType.Exception);
    
    
    public bool Failed => (ExceptionThrown || ErrorRaised);
    public bool Ok => !Failed;
    
    public Result<TNew> Copy<TNew>() => new(Message, Exception, type: Type);
}
