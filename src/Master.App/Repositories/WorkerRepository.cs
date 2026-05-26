using System.Collections.Concurrent;
using Master.App.EF;
using Master.Domain.Models;
using Master.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.DTOs;
using Shared.Domain.EF;

namespace Master.App.Repositories;

public class WorkerRepository(SchedulerDbContext context) : IWorkerRepository
{
    public IUnitOfWork UnitOfWork => context;

    public async Task<List<Worker>> AllAsync() => await context.Workers.ToListAsync();
    
    public async Task<int> CountAsync() => await context.Workers.CountAsync();
    
    public async Task<IMessage?> AddAsync(Worker worker)
    {
        _ = await context.Workers.AddAsync(worker);
        return null;
    }

    public async Task<IMessage?> RemoveAsync(Worker worker)
    {
        _ = context.Workers.Remove(worker);
        return await Task.FromResult<IMessage?>(null);
    }

    public async Task<(IMessage?, Worker?)> GetAsync(Guid workerId)
    {
        Worker? worker = await context.Workers.Where(w => w.Id == workerId).FirstOrDefaultAsync();
        if (worker is null)
        {
            return new(new WorkerRepositoryNotFoundError($"There is no worker with id ({workerId}) in the list"), null);
        }

        return (null, worker);
    }

    public async Task<(IMessage?, Worker?)> GetByNameAsync(string name)
    {
        Worker? worker = await context.Workers.Where(w => w.Name == name).FirstOrDefaultAsync();
        if (worker is null)
        {
            return new(new WorkerRepositoryNotFoundError($"There is no worker with name ({name}) in the list"), null);
        }

        return (null, worker);
    }
    
    public async Task<(IMessage?, Worker?)> GetDeadWorkerByNameAsync(string name)
    {
        Worker? worker = await context.Workers.Where(w => w.CurrentState == WorkerState.Dead && w.Name == name)
            .FirstOrDefaultAsync();
        if (worker is null)
        {
            return new(new WorkerRepositoryNotFoundError($"There is no worker with name ({name}) in the list"), null);
        }

        return (null, worker);
    }


    public async Task<(IMessage?, Worker?)> FirstAsync()
    {
        Worker? worker = await context.Workers.FirstOrDefaultAsync();
        if (worker is null)
        {
            return new(new WorkerRepositoryNotFoundError($"There is no worker in the list"), null);
        }

        return new(null, worker);
    }
}