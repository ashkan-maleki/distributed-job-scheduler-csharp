using Master.Domain.Aggregates;
using Shared.Domain.DTOs;

namespace Master.Domain.Services;

public interface IJobService
{
    public Task<IResult<Job>> QueueJob(string name);
    public Task<IResult<Job>> AssignJob(Guid workerId);
    public Task<IResult<Job>> StartJob(Guid jobId, Guid workerId);
    public Task<IResult<Job>> CompleteJob(Guid jobId, Guid workerId);
    public Task<IResult<Job>> FailJob(Guid jobId, Guid workerId);
    public Task<List<Job>> AllAsync();
}