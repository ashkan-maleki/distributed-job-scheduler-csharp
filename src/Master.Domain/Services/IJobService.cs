using Master.Domain.Aggregates;
using Shared.Domain.Failures;

namespace Master.Domain.Services;

public interface IJobService
{
    public (IError?, Job?) TryQueueJob(string name);
    public (IError?, Job?) TryAssignJob(Guid workerId);
    public (IError?, Job?) TryStartJob(Guid jobId, Guid workerId);
    public (IError?, Job?) TryCompleteJob(Guid jobId, Guid workerId);
    public (IError?, Job?) TryFailJob(Guid jobId, Guid workerId);
    public List<Job> Jobs { get; }
}