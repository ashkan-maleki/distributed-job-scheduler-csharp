using Master.Domain.Models;
using Shared.Domain.DTOs;

namespace Master.Domain.Services;

public interface IWorkerService
{
    public Task<List<Worker>> AllAsync();
    public Task<Result> ScaleAsync(int count);
    
    public Task<QueryResult<Worker>> RegisterAsync(string name);
    public Task<Result> UnregisterAsync(Worker worker);
    public Task<Result> ReportHeartBeatAsync(Guid workerId);
}