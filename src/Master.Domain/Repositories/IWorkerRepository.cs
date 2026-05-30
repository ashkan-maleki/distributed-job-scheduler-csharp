using Master.Domain.Models;
using Shared.Domain.Data;
using Shared.Domain.DTOs;

namespace Master.Domain.Repositories;

public interface IWorkerRepository : IRepository
{
    public Task<List<Worker>> AllAsync();
    public Task<int> CountAsync();
    
    public Task<Result2> AddAsync(Worker worker);
    public Task<Result2> RemoveAsync(Worker worker);
    
    public Task<QueryResult2<Worker>> GetAsync(Guid workerId);
    public Task<QueryResult2<Worker>> GetByNameAsync(string name);
    public Task<QueryResult2<Worker>> GetDeadWorkerByNameAsync(string name);
    public Task<QueryResult2<Worker>> FirstAsync();
}
