namespace DistributedJobScheduler.Shared;

public record Job(string Name)
{
    public Guid Id { get; init; } = Guid.NewGuid();
}
