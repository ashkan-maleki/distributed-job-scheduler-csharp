using Master.App.EF;
using Master.Domain.Models;
using Master.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.DTOs;
using Shared.Domain.EF;

namespace Master.App.Repositories;

public class WorkersStateRepository(SchedulerDbContext dbContext) : IWorkersStateRepository
{
    public IUnitOfWork UnitOfWork { get; } = dbContext;

    public async Task<Result<WorkersState>> GetAsync()
    {
        WorkersState? workersState = await dbContext.WorkersStates.FirstOrDefaultAsync();
        if (workersState is null)
        {
            return new NotFound("Scaling the number of workers didn't take place");
        }
        return workersState;
    }
}