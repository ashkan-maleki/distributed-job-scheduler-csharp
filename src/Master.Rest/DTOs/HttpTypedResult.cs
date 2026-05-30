using Microsoft.AspNetCore.Http.HttpResults;
using Shared.Domain.DTOs;

namespace Master.Rest.DTOs;

public static class HttpTypedResult
{
    private static IResult ToHttpIResultFromResult(Result2 result2, string? message = null)
    {
        if (result2.HasMessage || result2.ErrorRaised)
        {
            message ??= result2.Message;    
        }

        if (result2.Ok && string.IsNullOrWhiteSpace(message))
        {
            return TypedResults.Ok();
        }
        
        if (result2.DomainError)
        {
            return TypedResults.BadRequest(message);
        } 
        if (result2.NotFound)
        {
            return TypedResults.NotFound(message);
        } 
        return TypedResults.Ok();
    }

    public static IResult ToHttpIResult(this Result2 result2, string? message = null) 
        => ToHttpIResultFromResult(result2, message);

    public static IResult ToHttpIResult<T>(this QueryResult2<T> result2, string? message = null)
    {
        if (result2.TryGetData(out var data))
        {
            return TypedResults.Ok(data);
        }
        return ToHttpIResultFromResult(result2, message);
    }

    public static Results<T1, T2> From<T1, T2>
        (Result2 result2, string? message = null) where T1 : IResult where T2 : IResult
    {
        IResult res = result2.ToHttpIResult(message);
        var typedResult = (Results<T1, T2>)res;
        return typedResult;
    }
    
    public static Results<T1, T2> From<T, T1, T2>
        (QueryResult2<T> result2, string? message = null) where T1 : IResult where T2 : IResult
    {
        IResult res = result2.ToHttpIResult(message);
        var typedResult = (Results<T1, T2>)res;
        return typedResult;
    }
    
    public static Results<T1, T2, T3> From<T1, T2, T3>
        (Result2 result2, string? message = null) where T1 : IResult where T2 : IResult where T3 : IResult
    {
        IResult res = result2.ToHttpIResult(message);
        var typedResult = (Results<T1, T2, T3>)res;
        return typedResult;
    }
    
    public static Results<T1, T2, T3> From<T, T1, T2, T3>
        (QueryResult2<T> result2, string? message = null) where T1 : IResult where T2 : IResult where T3 : IResult
    {
        IResult res = result2.ToHttpIResult(message);
        var typedResult = (Results<T1, T2, T3>)res;
        return typedResult;
    }

}