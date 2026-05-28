using System.Diagnostics.CodeAnalysis;

namespace Shared.Domain.DTOs;

public static class Results
{
    public static Result<T> Ok<T>(T data) => new(data);
    public static Result<T> Saved<T>(string message) => new(message, status: ResultStatus.Saved);
    public static Result<T> NotFound<T>(string message) => new(message, status: ResultStatus.NotFound);
    public static Result<T> DomainErrorRaised<T>(string message) => new(message, ResultStatus.DomainError);
    public static Result<T> ExceptionThrown<T>(Exception exception) => new(exception);
}

public enum ResultStatus
{
    Ok,
    NoContent,
    Saved,
    NotFound,
    DomainError,
    UnexpectedError
}

public class Result<T>
{
    private bool Success => _status is ResultStatus.Ok 
        or ResultStatus.NoContent
        or ResultStatus.Saved;
    
    private readonly ResultStatus _status;

    public T? Data { get; }
    public string? Message { get; }
    
    public Exception? Exception { get; }
    
    
    private Result(string? message, Exception? exception, ResultStatus status)
    {
        Message = message;
        Exception = exception;
        _status = status;
        
    }

    public Result(string message, ResultStatus status = ResultStatus.NotFound)
    {
        Message = message;
        Exception = null;
        _status = status;
        
    }

    public Result(Exception exception)
    {
        Message = exception.Message;
        Exception = exception;
        _status = ResultStatus.UnexpectedError;
    }

    public Result(T data)
    {
        Message = string.Empty;
        Data = data;
        Exception = null;
        _status = ResultStatus.Ok;
    }
    
    [MemberNotNullWhen(true, nameof(Data))]
    public bool Ok => (Data is not null && _status == ResultStatus.Ok);
    
    [MemberNotNullWhen(true, nameof(Exception))]
    public bool ExceptionThrown => (Exception is not null && _status == ResultStatus.UnexpectedError);

    [MemberNotNullWhen(true, nameof(Message))]
    public bool HasMessage => (!string.IsNullOrEmpty(Message) && Success);

    [MemberNotNullWhen(true, nameof(Message))]
    public bool ErrorRaised => (!string.IsNullOrEmpty(Message) && !Success && !ExceptionThrown);
    
    public bool TryOk([NotNullWhen(true)] out T? data)
    {
        data = Data;
        return Ok;
    }

    public bool TryException([NotNullWhen(true)] out Exception? exception)
    {
        exception = Exception;
        return ExceptionThrown;
    }

    public bool TryMessage([NotNullWhen(true)] out string? message)
    {
        message = Message;
        return HasMessage;
    }

    public bool TryError([NotNullWhen(true)] out string? message)
    {
        message = Message;
        return ErrorRaised;
    }
    
    
    public Result<TNew> SwapPayload<TNew>() => new(Message, Exception, status: _status);
}
