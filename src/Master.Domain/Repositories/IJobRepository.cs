using Shared.Domain.Data;
using Shared.Domain.Failures;
using Job = Master.Domain.Aggregates.Job;

namespace Master.Domain.Repositories;

public interface IJobRepository : IRepository
{
    public List<Job> Jobs { get; }

    public Task<IError?> AddAsync(Job job);
    public Task<(IError?, Job?)> DequeueAsync();
    // public Task<IError?> Update(Job newJob, Job oldJob);
    public Task<(IError?, Job?)> GetAsync(Guid jobId);
}

public class JobRepositoryOperationError(string message) : Error<IJobRepository>(message);
public class JobRepositoryNotFoundError(string message) : Error<IJobRepository>(message);