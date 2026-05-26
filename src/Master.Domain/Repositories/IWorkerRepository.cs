using Master.Domain.Models;
using Shared.Domain.Data;
using Shared.Domain.DTOs;

namespace Master.Domain.Repositories;

public interface IWorkerRepository : IRepository
{
    public Task<List<Worker>> AllAsync();
    public Task<int> CountAsync();
    
    public Task<IMessage?> AddAsync(Worker worker);
    public Task<IMessage?> RemoveAsync(Worker worker);
    
    public Task<(IMessage?, Worker?)> GetAsync(Guid workerId);
    public Task<(IMessage?, Worker?)> GetByNameAsync(string name);
    public Task<(IMessage?, Worker?)> GetDeadWorkerByNameAsync(string name);
    public Task<(IMessage?, Worker?)> FirstAsync();
}

public class WorkerRepositoryOperationError(string content) : Error<IWorkerRepository>(content);
public class WorkerRepositoryNotFoundError(string content) : Error<IWorkerRepository>(content);