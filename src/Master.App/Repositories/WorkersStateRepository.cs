using Master.App.EF;
using Master.Domain.Models;
using Master.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.DTOs;

namespace Master.App.Repositories;

public class WorkersStateRepository(SchedulerDbContext dbContext) : Repository(dbContext), IWorkersStateRepository
{
    public async Task<Result<WorkersState>> GetAsync()
    {
        WorkersState? workersState = await DbContext.WorkersStates.FirstOrDefaultAsync();
        if (workersState is null)
        {
            return new NotFound("Scaling the number of workers didn't take place");
        }
        return workersState;
    }
}