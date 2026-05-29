using Microsoft.AspNetCore.Http.HttpResults;
using Shared.Domain.DTOs;

namespace Master.Rest.DTOs;

public static class HttpTypedResult
{
    private static IResult ToHttpIResultFromResult(Result result, string? message = null)
    {
        if (result.HasMessage || result.ErrorRaised)
        {
            message ??= result.Message;    
        }

        if (result.Ok && string.IsNullOrWhiteSpace(message))
        {
            return TypedResults.Ok();
        }
        
        if (result.DomainError)
        {
            return TypedResults.BadRequest(message);
        } 
        if (result.NotFound)
        {
            return TypedResults.NotFound(message);
        } 
        return TypedResults.Ok();
    }

    public static IResult ToHttpIResult(this Result result, string? message = null) 
        => ToHttpIResultFromResult(result, message);

    public static IResult ToHttpIResult<T>(this QueryResult<T> result, string? message = null)
    {
        if (result.TryGetData(out var data))
        {
            return TypedResults.Ok(data);
        }
        return ToHttpIResultFromResult(result, message);
    }

    public static Results<T1, T2> From<T1, T2>
        (Result result, string? message = null) where T1 : IResult where T2 : IResult
    {
        IResult res = result.ToHttpIResult(message);
        var typedResult = (Results<T1, T2>)res;
        return typedResult;
    }
    
    public static Results<T1, T2> From<T, T1, T2>
        (QueryResult<T> result, string? message = null) where T1 : IResult where T2 : IResult
    {
        IResult res = result.ToHttpIResult(message);
        var typedResult = (Results<T1, T2>)res;
        return typedResult;
    }
    
    public static Results<T1, T2, T3> From<T1, T2, T3>
        (Result result, string? message = null) where T1 : IResult where T2 : IResult where T3 : IResult
    {
        IResult res = result.ToHttpIResult(message);
        var typedResult = (Results<T1, T2, T3>)res;
        return typedResult;
    }
    
    public static Results<T1, T2, T3> From<T, T1, T2, T3>
        (QueryResult<T> result, string? message = null) where T1 : IResult where T2 : IResult where T3 : IResult
    {
        IResult res = result.ToHttpIResult(message);
        var typedResult = (Results<T1, T2, T3>)res;
        return typedResult;
    }

}