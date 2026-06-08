namespace Worker.Rest.Domain;

public class Worker
{
    public Guid Id { get; init; }
    public string Name { get; init; } = "";
    public DateTime RegisteredAt { get; private set; }
    public DateTime HeartBeatReportedAt { get; private set; }
    public DateTime JobCompletedAt { get; private set; }
    public DateTime JobAssignedAt { get; private set; }
    public Guid JobId { get; private set; } = Guid.Empty;

    public void Register()
    {
        RegisteredAt = DateTime.UtcNow;
    }

    public void ReportHeartBeat()
    {
        HeartBeatReportedAt = DateTime.UtcNow;
    }

    public void CompleteJob()
    {
        JobCompletedAt = DateTime.UtcNow;
    }

    public void AssignJob(Guid jobId)
    {
        JobId = jobId;
        JobAssignedAt = DateTime.UtcNow;
    }

    public bool ShouldReportHeartBeat => HeartBeatReportedAt - DateTime.UtcNow > TimeSpan.FromSeconds(4);
    public bool ShouldNotReportHeartBeat => !ShouldReportHeartBeat;
    public bool IsJobAssigned => JobId != Guid.Empty;
    public bool ReadyToProcessJob => IsJobAssigned;

    public void StartJob(Guid jobId)
    {
        // TODO
    }
}