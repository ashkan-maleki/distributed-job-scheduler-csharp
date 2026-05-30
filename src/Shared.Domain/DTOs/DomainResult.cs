using System.Diagnostics.CodeAnalysis;

namespace Shared.Domain.DTOs;

public enum DomainResultStatus
{
    Valid,
    Invalid,
}

public class DomainResult
{
    private DomainResultStatus Status { get; }
    private string? Message { get; }
    
    private DomainResult(string message)
    {
        Message = message;
        Status = DomainResultStatus.Invalid;
    }
    
    private DomainResult()
    {
        Status = DomainResultStatus.Valid;
    }
    
    [MemberNotNullWhen(true, nameof(Message))]
    public bool Invalid => Status == DomainResultStatus.Invalid;

    public bool TryGetError([NotNullWhen(true)] out string? error)
    {
        error = Message;
        return Status == DomainResultStatus.Invalid;
    }
    
    public static DomainResult Ok() => new();
    public static DomainResult Error(string message) => new(message);
}