namespace Shared.Domain.Failures;

public class Error<T>(string message, IError? innerError = null) : IError
{
    public string Message { get;  } = message;
    public IError? InnerError  { get;  } = innerError;
    public Type Type { get; init; } = typeof(T);

    public override string ToString()
    {
        List<string> errors = [];

        IError? current = this;

        while (current is not null)
        {
            errors.Add(
                $"[{current.Type.Name}] {current.Message}");

            current = current.InnerError;
        }

        return string.Join(
            " --> ",
            errors);
    }
    
    public bool Is<T>() => GetType() == typeof(T);
    public bool As<T>() => Type == typeof(T);
}