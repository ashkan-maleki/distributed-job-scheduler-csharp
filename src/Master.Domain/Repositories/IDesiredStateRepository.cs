using Master.Domain.Models;
using Shared.Domain.Data;
using Shared.Domain.DTOs;

namespace Master.Domain.Repositories;

public interface IDesiredStateRepository : IRepository
{
    public Task AddAsync(DesiredState desiredState);
    public void Remove(DesiredState desiredState);
    public Task<Result<DesiredState>> GetAsync();
}

