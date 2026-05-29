using Master.Domain.Aggregates;
using Shared.Domain.DTOs;

namespace Master.Domain.Services;

public interface IJobService
{
    public Task<QueryResult<Job>> QueueJob(string name);
    public Task<QueryResult<Job>> AssignJob(Guid workerId);
    public Task<QueryResult<Job>> StartJob(Guid jobId, Guid workerId);
    public Task<QueryResult<Job>> CompleteJob(Guid jobId, Guid workerId);
    public Task<QueryResult<Job>> FailJob(Guid jobId, Guid workerId);
    public Task<List<Job>> AllAsync();
}