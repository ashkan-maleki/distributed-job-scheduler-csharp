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
    public async Task AddAsync(WorkersState workersState) => _ = await dbContext.WorkersStates.AddAsync(workersState);

    public async Task<Result<WorkersState>> GetAsync()
    {
        WorkersState? workersState = await dbContext.WorkersStates
            .Where(ws => ws.NumberOfWorkersToRegister > 0)
            .FirstOrDefaultAsync();
        if (workersState is null)
        {
            return new NotFound("Scaling the number of workers didn't take place");
        }
        return workersState;
    }

    public Result<WorkersState> Get()
    {
        WorkersState? workersState = dbContext.WorkersStates
            .Where(ws => ws.NumberOfWorkersToRegister > 0)
            .FirstOrDefault();
        if (workersState is null)
        {
            return new NotFound("Scaling the number of workers didn't take place");
        }
        return workersState;
    }
}