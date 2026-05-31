using Master.Domain.Models;
using Shared.Domain.Data;
using Shared.Domain.DTOs;

namespace Master.Domain.Repositories;

public interface IWorkerRepository : IRepository
{
    public Task<List<Worker>> AllAsync();
    public Task<int> CountAsync();
    
    public Task AddAsync(Worker worker);
    public Task RemoveAsync(Worker worker);
    
    public Task<IResult<Worker>> GetAsync(Guid workerId);
    public Task<IResult<Worker>> GetByNameAsync(string name);
    public Task<IResult<Worker>> GetDeadWorkerByNameAsync(string name);
    public Task<IResult<Worker>> FirstAsync();
}
