using Master.Domain.Models;
using Shared.Domain.Data;
using Shared.Domain.DTOs;

namespace Master.Domain.Repositories;

public interface IWorkerRepository : IRepository
{
    public Task<List<Worker>> AllAsync();
    public Task<int> CountAsync();
    public Task<bool> AnyAsync(Guid workerId);
    
    public Task AddAsync(Worker worker);
    public void Remove(Worker worker);
    
    public Task<Result<Worker>> GetAsync(Guid workerId);
    public Task<Result<Worker>> GetUnregisteredAsync();
    public Task<Result<Worker>> GetByNameAsync(string name);
    public Task<Result<Worker>> GetDeadWorkerByNameAsync(string name);
    public Task<Result<Worker>> FirstAsync();
}
