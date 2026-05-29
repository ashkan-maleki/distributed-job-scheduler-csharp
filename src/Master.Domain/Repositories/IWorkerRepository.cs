using Master.Domain.Models;
using Shared.Domain.Data;
using Shared.Domain.DTOs;

namespace Master.Domain.Repositories;

public interface IWorkerRepository : IRepository
{
    public Task<List<Worker>> AllAsync();
    public Task<int> CountAsync();
    
    public Task<Result> AddAsync(Worker worker);
    public Task<Result> RemoveAsync(Worker worker);
    
    public Task<QueryResult<Worker>> GetAsync(Guid workerId);
    public Task<QueryResult<Worker>> GetByNameAsync(string name);
    public Task<QueryResult<Worker>> GetDeadWorkerByNameAsync(string name);
    public Task<QueryResult<Worker>> FirstAsync();
}
