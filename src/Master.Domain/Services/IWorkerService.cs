using Master.Domain.Models;
using Shared.Domain.Failures;

namespace Master.Domain.Services;

public interface IWorkerService
{
    public Task<List<Worker>> AllAsync();
    public Task<IError?> ScaleAsync(int count);
    
    public Task<(IError?, Worker?)> RegisterAsync(string name);
    public Task<IError?> UnregisterAsync(Worker worker);
    public Task<IError?> ReportHeartBeatAsync(Guid workerId);
}

public class WorkerServiceInternalError(string message) : Error<IWorkerService>(message);
public class WaitingSignalForWorkersError(string message) : Error<IWorkerService>(message);