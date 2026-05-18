using Master.Domain.Aggregates;
using Shared.Domain.Failures;

namespace Master.Domain.Services;

public interface IJobService
{
    public Task<(IError?, Job?)> QueueJob(string name);
    public Task<(IError?, Job?)> AssignJob(Guid workerId);
    public Task<(IError?, Job?)> StartJob(Guid jobId, Guid workerId);
    public Task<(IError?, Job?)> CompleteJob(Guid jobId, Guid workerId);
    public Task<(IError?, Job?)> FailJob(Guid jobId, Guid workerId);
    public List<Job> Jobs { get; }
}