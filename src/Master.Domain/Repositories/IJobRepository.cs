using Shared.Domain.Data;
using Shared.Domain.DTOs;
using Job = Master.Domain.Aggregates.Job;

namespace Master.Domain.Repositories;

public interface IJobRepository : IRepository
{
    public Task<List<Job>> AllAsync();

    public Task<IMessage?> AddAsync(Job job);
    public Task<(IMessage?, Job?)> DequeueAsync();
    // public Task<IError?> Update(Job newJob, Job oldJob);
    public Task<(IMessage?, Job?)> GetAsync(Guid jobId);
}

public class JobRepositoryOperationError(string content) : Error<IJobRepository>(content);
public class JobRepositoryNotFoundError(string content) : Error<IJobRepository>(content);