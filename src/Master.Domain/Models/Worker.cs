using Master.Domain.Aggregates;

namespace Master.Domain.Models;

public record Worker(string Name, Job? Job = null)
{
    public Guid Id { get; init; } = Guid.NewGuid();
}