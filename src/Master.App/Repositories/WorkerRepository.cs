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
    public async Task<bool> AnyAsync(Guid workerId) => await context.Workers
        .Where(w => w.Id == workerId)
        .AnyAsync();


    public async Task AddAsync(Worker worker) => _ = await context.Workers.AddAsync(worker);

    public void Remove(Worker worker) => _ = context.Workers.Remove(worker);

    private static Result<Worker> CheckIfWorkerIsNull(Worker? worker, string notFoundError)
    {
        if (worker is null)
        {
            return new NotFound(notFoundError);
        }

        return worker;
    }

    public async Task<Result<Worker>> GetAsync(Guid workerId) => CheckIfWorkerIsNull(
        await context.Workers.Where(w => w.Id == workerId).FirstOrDefaultAsync(),
        $"There is no worker with id ({workerId}) in the list");

    public async Task<Result<Worker>> GetUnregisteredAsync() => CheckIfWorkerIsNull(
        await context.Workers.Where(w => w.RegisteredAt == DateTime.MinValue).FirstOrDefaultAsync(),
        "There is no unregistered worker");

    public async Task<Result<Worker>> GetByNameAsync(string name) =>
        CheckIfWorkerIsNull(await context.Workers.Where(w => w.Name == name).FirstOrDefaultAsync(),
            $"There is no worker with name ({name}) in the list");

    public async Task<Result<Worker>> GetDeadWorkerByNameAsync(string name) =>
        CheckIfWorkerIsNull(await context.Workers
                .Where(w => w.CurrentState == WorkerState.Dead && w.Name == name)
                .FirstOrDefaultAsync(),
            $"There is no worker with name ({name}) in the list");


    public async Task<Result<Worker>> FirstAsync() =>
        CheckIfWorkerIsNull(await context.Workers.FirstOrDefaultAsync(),
            $"There is no worker in the list");
}