using Master.Domain.Aggregates;

namespace Master.Domain.Models;

public record Worker(string Name)
{
    public Guid Id { get; init; } = Guid.NewGuid();
}