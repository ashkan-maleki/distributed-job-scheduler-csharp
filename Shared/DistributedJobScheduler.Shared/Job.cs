namespace DistributedJobScheduler.Shared;

public record Job(string Name)
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
