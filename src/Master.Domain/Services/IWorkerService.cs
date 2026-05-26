using Master.Domain.Models;
using Shared.Domain.DTOs;

namespace Master.Domain.Services;

public interface IWorkerService
{
    public Task<List<Worker>> AllAsync();
    public Task<IMessage?> ScaleAsync(int count);
    
    public Task<(IMessage?, Worker?)> RegisterAsync(string name);
    public Task<IMessage?> UnregisterAsync(Worker worker);
    public Task<IMessage?> ReportHeartBeatAsync(Guid workerId);
}

public class WorkerServiceInternalError(string content) : Error<IWorkerService>(content);
public class WaitingSignalForWorkersError(string content) : Error<IWorkerService>(content);