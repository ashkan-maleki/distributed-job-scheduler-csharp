using Master.Domain.Models;
using Shared.Domain.Data;
using Shared.Domain.Failures;

namespace Master.Domain.Repositories;

public interface IWorkerRepository : IRepository
{
    public List<Worker> Workers { get; }
    public int WorkersCount { get; }

    public Task<IError?> AddAsync(Worker worker);
    public Task<IError?> RemoveAsync(Worker worker);
    
    public Task<(IError?, Worker?)> GetAsync(Guid workerId);
    public Task<(IError?, Worker?)> FirstAsync();
}

public class WorkerRepositoryOperationError(string message) : Error<IWorkerRepository>(message);
public class WorkerRepositoryNotFoundError(string message) : Error<IWorkerRepository>(message);