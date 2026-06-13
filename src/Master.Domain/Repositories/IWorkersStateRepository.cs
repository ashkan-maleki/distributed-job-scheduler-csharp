using Master.Domain.Models;
using Shared.Domain.Data;
using Shared.Domain.DTOs;

namespace Master.Domain.Repositories;

public interface IWorkersStateRepository : IRepository
{
    public Task AddAsync(WorkersState workersState);
    public Task<Result<WorkersState>> GetAsync();
    public Result<WorkersState> Get();
}