using Master.App.EF;
using Master.Domain.Models;
using Master.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.DTOs;
using Shared.Domain.EF;

namespace Master.App.Repositories;

public class DesiredStateRepository(SchedulerDbContext dbContext) : IDesiredStateRepository
{
    public IUnitOfWork UnitOfWork => dbContext;
    public async Task AddAsync(DesiredState desiredState) =>
        _ = await dbContext.SchedulerStates.AddAsync(desiredState);

    public void Remove(DesiredState desiredState) => dbContext.SchedulerStates.Remove(desiredState);

    public async Task<Result<DesiredState>> GetAsync()
    {
        DesiredState? schedulerState = await dbContext.SchedulerStates.FirstOrDefaultAsync();
        if (schedulerState == null)
        {
            return new NotFound("not scheduler state is stored in db.");
        }

        return schedulerState;
    }
}