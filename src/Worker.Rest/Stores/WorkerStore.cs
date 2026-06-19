using Microsoft.AspNetCore.DataProtection.Repositories;
using Shared.Domain.Data;
using Shared.Domain.DTOs;
using Worker.Rest.EF;

namespace Worker.Rest.Stores;




public class WorkerStore(WorkerDbContext dbContext) : Store(dbContext), IWorkerStore
{
    public async Task AddAsync(Domain.Worker worker, CancellationToken stoppingToken) 
        => await dbContext.Workers.AddAsync(worker, stoppingToken);

    public async Task<Result<Domain.Worker>> FindAsync(Guid workerId, CancellationToken stoppingToken)
    {
        Domain.Worker? worker = await dbContext.Workers.FindAsync(workerId, stoppingToken);
        if (worker == null)
        {
            return new NotFound($"Worker with id, {workerId} was not found.");
        }
        return worker;
    }
}

public interface IWorkerStore : IRepository
{
    Task AddAsync(Domain.Worker worker, CancellationToken stoppingToken);
    Task<Result<Domain.Worker>> FindAsync(Guid workerId, CancellationToken stoppingToken);
}