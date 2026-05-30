namespace Shared.Domain.DTOs;



public interface IResult
{
    bool Success { get; }   
    bool Failure { get; }
}

public interface IText : IResult
{
    string Message { get; }
}

public interface IResult<T> : IResult;

public interface IText<T> : IResult<T>, IText;

public interface IObject<T> : IResult<T>
{
    T Value { get; }
}


public abstract class Result(bool success) : IResult
{
    public virtual bool Success { get; } = success;
    public virtual bool Failure => !Success;
}



public abstract class Text(bool success, string message) : Result(success), IText
{
    public string Message => message;
}

public class Success(string message) : Text(true, message);
public class Error(string error) : Text(false, error);

public abstract class Result<T>(bool success, T value) : Result(success), IObject<T>
{
    public T Value => value;
}

public abstract class Text<T>(bool success, string message): Result(success), IText<T>
{
    public string Message => message;
}

public class Ok() : Result(true);

public class CriticalError(string error) : Error(error);

public class Ok<T>(T value) : Result<T>(true, value);
public class Error<T>(string error) : Text<T>(false, error);


