using Master.Domain.Aggregates;
using Shared.Domain.DTOs;

namespace Master.Domain.Services;

public interface IJobService
{
    public Task<QueryResult2<Job>> QueueJob(string name);
    public Task<QueryResult2<Job>> AssignJob(Guid workerId);
    public Task<QueryResult2<Job>> StartJob(Guid jobId, Guid workerId);
    public Task<QueryResult2<Job>> CompleteJob(Guid jobId, Guid workerId);
    public Task<QueryResult2<Job>> FailJob(Guid jobId, Guid workerId);
    public Task<List<Job>> AllAsync();
}