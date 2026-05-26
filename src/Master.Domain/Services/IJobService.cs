using Master.Domain.Aggregates;
using Shared.Domain.DTOs;

namespace Master.Domain.Services;

public interface IJobService
{
    public Task<(IMessage?, Job?)> QueueJob(string name);
    public Task<(IMessage?, Job?)> AssignJob(Guid workerId);
    public Task<(IMessage?, Job?)> StartJob(Guid jobId, Guid workerId);
    public Task<(IMessage?, Job?)> CompleteJob(Guid jobId, Guid workerId);
    public Task<(IMessage?, Job?)> FailJob(Guid jobId, Guid workerId);
    public Task<List<Job>> AllAsync();
}