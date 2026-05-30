namespace Shared.Domain.DTOs;

public abstract class BaseContentMessage(string content) : IContentMessage
{
    protected BaseContentMessage() : this(string.Empty)
    {
    }

    public string Content { get; } = content;

    public override string ToString()
    {
        return Content;
    }

    public bool Is<TMessage>() => GetType() == typeof(TMessage);
}

public class EmptyContentMessage : BaseContentMessage;

public class ContentMessage(string content) : BaseContentMessage(content);

public class ErrorContentMessage(string content) : BaseContentMessage(content);

