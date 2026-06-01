using Master.Domain.Models;
using Shared.Domain.DTOs;

namespace Master.Domain.Services;

public interface IWorkerService
{
    public Task<List<Worker>> AllAsync();
    public Task<IResult> ScaleAsync(int count);
    
    public Task<Result<Worker>> RegisterAsync(string name);
    public Task<IResult> UnregisterAsync(Worker worker);
    public Task<IResult> ReportHeartBeatAsync(Guid workerId);
}