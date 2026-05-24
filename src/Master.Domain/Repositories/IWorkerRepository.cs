using Master.Domain.Models;
using Shared.Domain.Data;
using Shared.Domain.Messages;

namespace Master.Domain.Repositories;

public interface IWorkerRepository : IRepository
{
    public Task<List<Worker>> AllAsync();
    public Task<int> CountAsync();
    
    public Task<IError?> AddAsync(Worker worker);
    public Task<IError?> RemoveAsync(Worker worker);
    
    public Task<(IError?, Worker?)> GetAsync(Guid workerId);
    public Task<(IError?, Worker?)> GetByNameAsync(string name);
    public Task<(IError?, Worker?)> GetDeadWorkerByNameAsync(string name);
    public Task<(IError?, Worker?)> FirstAsync();
}

public class WorkerRepositoryOperationError(string message) : Error<IWorkerRepository>(message);
public class WorkerRepositoryNotFoundError(string message) : Error<IWorkerRepository>(message);