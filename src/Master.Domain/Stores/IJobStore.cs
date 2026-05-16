using Shared.Domain.Failures;
using Job = Master.Domain.Aggregates.Job;

namespace Master.Domain.Stores;

public interface IJobStore
{
    public List<Job> Jobs { get; }

    public IError? TryAddJob(Job job);
    public (IError?, Job?) TryDequeueJob();
    public IError? TryUpdateJob(Job newJob, Job oldJob);
    public (IError?, Job?) TryGetJob(Guid jobId);
}

public class JobStoreOperationError(string message) : Error<IJobStore>(message);
public class JobStoreNotFoundError(string message) : Error<IJobStore>(message);