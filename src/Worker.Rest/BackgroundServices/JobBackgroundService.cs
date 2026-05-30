using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Shared.Domain.DTOs;
using Shared.Domain.Models;
using Worker.Rest.Contexts;
using Worker.Rest.EF;
using Worker.Rest.HttpServices.Master;
using Worker.Rest.Works;

namespace Worker.Rest.BackgroundServices;

public class JobBackgroundService(
    IJobHttpClient jobHttpClient,
    IWorkerHttpClient workerHttpClient,
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
            
            
            Domain.Worker? worker = await dbContext.Workers
                .Where(w => w.JobId == null)
                .OrderBy(w => w.JobCompletedAt)
                .FirstOrDefaultAsync(cancellationToken: stoppingToken);
            
            

            if (worker == null)
            {
                logger.LogWarning("Worker not found.");
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                continue;
            }

            (IContentMessage? error, Job? job) = await jobHttpClient.GetJobAsync(worker.Id, stoppingToken);
            if (error != null)
            {
                logger.LogError(error.ToString());
                continue;
            }

            if (job == null)
            {
                logger.LogError("Instance of job is null");
                continue;
            }

            worker.AssignJob(job.Id);
            await dbContext.SaveChangesAsync(stoppingToken);

            (error, job) = await jobHttpClient.StartJobAsync(worker.Id, job.Id, stoppingToken);

            if (error != null)
            {
                logger.LogError(error.ToString());
                continue;
            }

            if (job == null)
            {
                logger.LogError("Instance of job is null");
                continue;
            }

            worker.StartJob(job.Id);
            await dbContext.SaveChangesAsync(stoppingToken);

            await SimpleWork.Run(stoppingToken);
            
            JobResultRequest request = new(worker.Id, job.Id, true, null);
            (error, job) = await jobHttpClient.ResultJobAsync(request, stoppingToken);

            if (error != null)
            {
                logger.LogError(error.ToString());
                continue;
            }

            if (job == null)
            {
                logger.LogError("Instance of job is null");
                continue;
            }
            
            worker.CompleteJob();
            await dbContext.SaveChangesAsync(stoppingToken);
        }
    }
}