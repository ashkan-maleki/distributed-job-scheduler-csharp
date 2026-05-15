namespace Shared.Domain.Models;


public record Job(string Name, Guid? WorkerId = null)
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public JobState State { get; init; } = JobState.Queued;
}

public enum JobState
{
    Queued,
    Assigned,
    Running,
    Completed,
    Failed,
}