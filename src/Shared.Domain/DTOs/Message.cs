namespace Shared.Domain.DTOs;

public abstract class BaseMessage(string content) : IMessage
{
    protected BaseMessage() : this(string.Empty)
    {
    }

    public string Content { get; } = content;

    public override string ToString()
    {
        return Content;
    }

    public bool Is<TMessage>() => GetType() == typeof(TMessage);
}

public class EmptyMessage : BaseMessage;

public class Message(string content) : BaseMessage(content);

public class Error(string content) : BaseMessage(content);

