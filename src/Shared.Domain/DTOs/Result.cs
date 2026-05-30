namespace Shared.Domain.DTOs;



public interface IResult
{
    bool Success { get; }   
    bool Failure { get; }
}

public interface IMessage : IResult
{
    string Content { get; }
}

public interface IResult<T> : IResult;

public interface IMessage<T> : IResult<T>, IMessage;

public interface IValue<T> : IResult<T>
{
    T Value { get; }
}


public abstract class Result(bool success) : IResult
{
    public virtual bool Success { get; } = success;
    public virtual bool Failure => !Success;
}



public abstract class Message(bool success, string content) : Result(success), IMessage
{
    public string Content => content;
}

public class Success(string content) : Message(true, content);
public class Error(string error) : Message(false, error);

public abstract class Result<T>(bool success, T value) : Result(success), IValue<T>
{
    public T Value => value;
}

public abstract class Message<T>(bool success, string content): Result(success), IMessage<T>
{
    public string Content => content;
}


public class Ok<T>(T value) : Result<T>(true, value);
public class Error<T>(string error) : Message<T>(false, error);


public class Ok() : Result(true);