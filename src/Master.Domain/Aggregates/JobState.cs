namespace Master.Domain.Aggregates;

public enum JobState
{
    Queued,
    Assigned,
    Running,
    Completed,
    Failed,
}