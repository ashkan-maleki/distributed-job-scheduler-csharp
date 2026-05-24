using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Shared.Domain.Messages;
using Shared.Domain.Models;
using Worker.Rest.Contexts;
using Worker.Rest.EF;
using Worker.Rest.HttpServices.Master;

namespace Worker.Rest.BackgroundServices;

public class JobBackgroundService(
    IJobHttpClient httpClient,
    ILogger<JobBackgroundService> logger,
    IDbContextFactory<WorkerDbContext> dbContextFactory,
    WorkerContext context) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (context.MasterUnavailable)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                continue;
            }

            await using WorkerDbContext dbContext = await dbContextFactory.CreateDbContextAsync(stoppingToken);
            Domain.Worker? worker = await dbContext.Workers.Where(w => w.JobId != null)
                .OrderBy(w => w.JobCompletedAt)
                .FirstOrDefaultAsync(cancellationToken: stoppingToken);

            if (worker == null)
            {
                logger.LogWarning("Worker not found.");
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                continue;
            }

            (IError? error, Job? job) = await httpClient.GetJobAsync(worker.Id, stoppingToken);
            if (error != null)
            {
                logger.LogError(error.ToString());
                continue;
            }

            worker.AssignJob(job!.Id);
        }
    }
}