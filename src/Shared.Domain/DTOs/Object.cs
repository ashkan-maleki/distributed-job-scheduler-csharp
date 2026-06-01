using System.Diagnostics.CodeAnalysis;

namespace Shared.Domain.DTOs;



public interface IResult
{
    bool Success { get; }   
    bool Failure { get; }
}


public abstract class Result(bool success) : IResult
{
    public virtual bool Success { get; } = success;
    public virtual bool Failure => !Success;
}
public class Ok() : Result(true);

public interface IText
{
    string Message { get; }
}

public abstract class Text(bool success, string message) : Result(success), IText
{
    public string Message => message;
}

public class Success(string message) : Text(true, message);

public class Error(string error) : Text(false, error);
public class NotFound(string error) : Error(error);
public class DomainFailure(string error) : Error(error);

public class CriticalError(string error) : Error(error);



// public interface IResult<T> : IResult;



public interface IResult<T> 
{
    T Value { get; }
}


public abstract class Object<T>(bool success, T value) : Result(success), IResult<T>
{
    public T Value => value;
    
    public static implicit operator T(Object<T> result) => result.Value;
}

class Ok<T>(T value) : Object<T>(true, value);


public class Result<T> : Result
{
    private readonly IResult _result;
    private Result(IResult result) : base(result.Success) => _result = result;
    

    public static implicit operator Result<T>(T value)
    {
        return new Result<T>(new Ok<T>(value));
    }
    
    public static implicit operator Result<T>(Object<T> @object)
    {
        return new Result<T>(@object);
    }
    
    public static implicit operator Result<T>(Text text)
    {
        return new Result<T>(text);
    }
   
    public static implicit operator T(Result<T> result)
    {
        if (result._result is Object<T> @object)
        {
            return @object;
        }
        return default!;
    }
}

public static class ResultExtensions
{
    public static bool TryGetValue<T>(this IResult result,[NotNullWhen(true)] out T? value)
    {
        value = default;
        if (result is Object<T> @object)
        {
            value = @object.Value;
        }
        return result is Object<T>;
    }
}







//
//
//
// public interface IText<T> : IResult<T>, IText;
//
//
// public abstract class Text<T>(bool success, string message): Result(success), IText<T>
// {
//     public string Message => message;
// }
// public class NotFound<T>(string error) : Text<T>(true, error);
// public class Error<T>(string error) : Text<T>(false, error);
// public class CriticalError<T>(string error) : Error<T>(error);
// public class UnknownError<T>(string error) : Error<T>(error);
