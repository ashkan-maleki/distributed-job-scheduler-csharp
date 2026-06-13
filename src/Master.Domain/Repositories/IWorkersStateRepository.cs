using Master.Domain.Models;
using Shared.Domain.Data;
using Shared.Domain.DTOs;

namespace Master.Domain.Repositories;

public interface IWorkersStateRepository : IRepository
{
    public Task Add(WorkersState workersState);
    public Task<Result<WorkersState>> Get();
}