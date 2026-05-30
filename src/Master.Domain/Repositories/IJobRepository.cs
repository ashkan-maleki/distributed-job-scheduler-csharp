using Shared.Domain.Data;
using Shared.Domain.DTOs;
using Job = Master.Domain.Aggregates.Job;

namespace Master.Domain.Repositories;

public interface IJobRepository : IRepository
{
    public Task<List<Job>> AllAsync();
    public Task<QueryResult2<Job>> GetAsync(Guid jobId);
    public Task<QueryResult2<Job>> GetQueuedJobAsync();
    public Task<Result2> AddAsync(Job job);
    
    // public Task<IError?> Update(Job newJob, Job oldJob);
    
}
