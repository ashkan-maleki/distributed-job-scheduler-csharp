using System.Diagnostics.CodeAnalysis;

namespace Shared.Domain.DTOs;







public enum DomainResultStatus
{
    Success,
    Failure,
}

public class DomainResult
{
    private DomainResultStatus Status { get; }
    private string? Content { get; }
    
    private DomainResult(string message)
    {
        Content = message;
        Status = DomainResultStatus.Failure;
    }
    
    private DomainResult()
    {
        Status = DomainResultStatus.Success;
    }
    
    [MemberNotNullWhen(true, nameof(Content))]
    public bool Failure => Status == DomainResultStatus.Failure;

    [MemberNotNullWhen(true, nameof(Content))]
    public bool Success => Status == DomainResultStatus.Success;
    
    public bool TryGetError([NotNullWhen(true)] out string? error)
    {
        error = Content;
        return Status == DomainResultStatus.Failure;
    }
    
    public static DomainResult Ok() => new();
    public static DomainResult Error(string message) => new(message);
}