using Shared.Domain.Data;
using Shared.Domain.DTOs;
using Job = Master.Domain.Aggregates.Job;

namespace Master.Domain.Repositories;

public interface IJobRepository : IRepository
{
    public Task<List<Job>> AllAsync();

    public Task<Result> AddAsync(Job job);
    public Task<QueryResult<Job>> DequeueAsync();
    // public Task<IError?> Update(Job newJob, Job oldJob);
    public Task<QueryResult<Job>> GetAsync(Guid jobId);
}
