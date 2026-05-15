namespace Shared.Domain.Failures;

public interface IError
{
    string Message { get; }
    IError? InnerError  { get; }
    Type Type  { get; }
    public bool Is<T>();
    public bool As<T>();
}