using Master.Domain.Models;
using Shared.Domain.DTOs;

namespace Master.Domain.Services;

public interface IWorkerService
{
    public Task<List<Worker>> AllAsync();
    public Task<Result2> ScaleAsync(int count);
    
    public Task<QueryResult2<Worker>> RegisterAsync(string name);
    public Task<Result2> UnregisterAsync(Worker worker);
    public Task<Result2> ReportHeartBeatAsync(Guid workerId);
}