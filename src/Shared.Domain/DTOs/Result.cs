using System.Diagnostics.CodeAnalysis;

namespace Shared.Domain.DTOs;

public static class Results
{
    public static Result NoContent() => new Result(ResultStatus.NoContent);
    public static Result Ok(string message) => new(message, status: ResultStatus.Ok);
    public static Result NotFound(string message) => new(message, status: ResultStatus.NotFound);
    public static Result DomainErrorRaised(string message) => new(message, ResultStatus.DomainError);
    public static Result ExceptionThrown(Exception exception) => new(exception);
}

public static class QueryResults
{
    public static QueryResult<T> NoContent<T>() => Results.NoContent().ToQueryResult<T>();
    public static QueryResult<T> Found<T>(T data) => new(data);
    public static QueryResult<T> Ok<T>(string message) => Results.Ok(message).ToQueryResult<T>();
    public static QueryResult<T> NotFound<T>(string message) => Results.NotFound(message).ToQueryResult<T>();

    public static QueryResult<T> DomainErrorRaised<T>(string message) =>
        Results.DomainErrorRaised(message).ToQueryResult<T>();

    public static QueryResult<T> ExceptionThrown<T>(Exception exception) =>
        Results.ExceptionThrown(exception).ToQueryResult<T>();
}

public enum ResultStatus
{
    Ok,
    Found,
    NoContent,
    NotFound,
    DomainError,
    UnexpectedError
}

public class Result
{
    private bool Success => Status is ResultStatus.Found
        or ResultStatus.NoContent
        or ResultStatus.Ok;

    public ResultStatus Status { get; protected set; }

    public string? Message { get; }

    public Exception? Exception { get; }


    protected internal Result(ResultStatus status)
    {
        Message = string.Empty;
        Exception = null;
        Status = status;
    }

    protected Result(string? message, Exception? exception, ResultStatus status)
    {
        Message = message;
        Exception = exception;
        Status = status;
    }

    protected internal Result(string message, ResultStatus status = ResultStatus.NotFound)
    {
        Message = message;
        Exception = null;
        Status = status;
    }

    protected internal Result(Exception exception)
    {
        Message = exception.Message;
        Exception = exception;
        Status = ResultStatus.UnexpectedError;
    }

    [MemberNotNullWhen(true, nameof(Exception))]
    public bool ExceptionThrown => (Exception is not null && Status == ResultStatus.UnexpectedError);

    [MemberNotNullWhen(true, nameof(Message))]
    public bool HasMessage => (!string.IsNullOrEmpty(Message) && Success);

    [MemberNotNullWhen(true, nameof(Message))]
    public bool ErrorRaised => (!string.IsNullOrEmpty(Message) && !Success && !ExceptionThrown);

    public bool TryGetException([NotNullWhen(true)] out Exception? exception)
    {
        exception = Exception;
        return ExceptionThrown;
    }

    public bool TryGetMessage([NotNullWhen(true)] out string? message)
    {
        message = Message;
        return HasMessage;
    }

    public bool TryGetError([NotNullWhen(true)] out string? message)
    {
        message = Message;
        return ErrorRaised;
    }
}

public class QueryResult<T> : Result
{
    public T? Data { get; }


    protected internal QueryResult(string? message, Exception? exception, ResultStatus status)
        : base(message, exception, status)
    {
    }
    

    protected internal QueryResult(T data) : base(ResultStatus.Found)
    {
        Data = data;
    }

    [MemberNotNullWhen(true, nameof(Data))]
    public bool Found => (Data is not null && Status == ResultStatus.Found);


    public bool TryGetData([NotNullWhen(true)] out T? data)
    {
        data = Data;
        return Found;
    }


    public QueryResult<TNew> SwapPayload<TNew>() => new(Message, Exception, status: Status);
}

public static class QueryResultsExtensions
{
    public static QueryResult<T> ToQueryResult<T>(this Result result)
        => new(result.Message, result.Exception, result.Status);
}