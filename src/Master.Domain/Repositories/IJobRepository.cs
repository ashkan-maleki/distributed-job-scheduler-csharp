using Shared.Domain.Data;
using Shared.Domain.DTOs;
using Job = Master.Domain.Aggregates.Job;

namespace Master.Domain.Repositories;

public interface IJobRepository : IRepository
{
    public Task<List<Job>> AllAsync();
    public Task<QueryResult<Job>> GetAsync(Guid jobId);
    public Task<QueryResult<Job>> GetQueuedJobAsync();
    public Task<Result> AddAsync(Job job);
    
    // public Task<IError?> Update(Job newJob, Job oldJob);
    
}
