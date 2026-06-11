using Master.Domain.Models;
using Shared.Domain.DTOs;

namespace Master.Domain.Services;

public interface IWorkerService
{
    public Task<List<Worker>> AllAsync();
    
    public Task<Result<Worker>> RegisterAsync();
    
    public Task<IResult> ReportHeartBeatAsync(Guid workerId);
    Task<IResult> ScaleAsync(int count);
    public Task<bool> CommitSuicideAsync(Guid workerId);
}