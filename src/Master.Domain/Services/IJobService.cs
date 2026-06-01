using Master.Domain.Aggregates;
using Shared.Domain.DTOs;

namespace Master.Domain.Services;

public interface IJobService
{
    public Task<Result<Job>> QueueJobAsync(string name);
    public Task<Result<Job>> AssignJobAsync(Guid workerId);
    public Task<Result<Job>> StartJobAsync(Guid jobId, Guid workerId);
    public Task<Result<Job>> CompleteJobAsync(Guid jobId, Guid workerId);
    public Task<Result<Job>> FailJobAsync(Guid jobId, Guid workerId);
    public Task<List<Job>> AllAsync();
}