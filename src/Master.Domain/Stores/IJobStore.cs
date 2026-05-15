using Shared.Domain.Failures;
using Job = Master.Domain.Aggregates.Job;

namespace Master.Domain.Stores;

public interface IJobStore
{
    public (IError?, Job?) TryQueueJob(string name);
    public (IError?, Job?) TryAssignJob(Guid workerId);
    public (IError?, Job?) TryStartJob(Guid jobId, Guid workerId);
    public (IError?, Job?) TryCompleteJob(Guid jobId, Guid workerId);
    public (IError?, Job?) TryFailJob(Guid jobId, Guid workerId);
}

public class JobStoreOperationError(string message) : Error<IJobStore>(message);
public class JobStoreNotFoundError(string message) : Error<IJobStore>(message);