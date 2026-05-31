using Master.Domain.Aggregates;
using Shared.Domain.DTOs;

namespace Master.Domain.Services;

public interface IJobService
{
    public Task<IResult<Job>> QueueJobAsync(string name);
    public Task<IResult<Job>> AssignJobAsync(Guid workerId);
    public Task<IResult<Job>> StartJobAsync(Guid jobId, Guid workerId);
    public Task<IResult<Job>> CompleteJobAsync(Guid jobId, Guid workerId);
    public Task<IResult<Job>> FailJobAsync(Guid jobId, Guid workerId);
    public Task<List<Job>> AllAsync();
}