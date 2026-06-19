using Microsoft.EntityFrameworkCore;
using Shared.Domain.DTOs;
using Worker.Rest.Contexts;
using Worker.Rest.EF;
using Worker.Rest.HttpServices.Master;

namespace Worker.Rest.Services;

public class Zarf(
    ILogger<Zarf> logger,
    IWorkerService workerService,
    IWorkerHttpClient workerHttpClient,
    IJobHttpClient jobHttpClient,
    WorkerDbContext dbContext)
{
    public static async Task<Result<Zarf>> RegisterAsync(ILogger<Zarf> logger,
        IWorkerService workerService,
        IWorkerHttpClient workerHttpClient,
        IJobHttpClient jobHttpClient, IDbContextFactory<WorkerDbContext> factory,
        WorkerContext context, CancellationToken stoppingToken)
    {
        await using WorkerDbContext dbContext = await factory.CreateDbContextAsync(stoppingToken);
        Zarf zarf = new(logger, workerService, workerHttpClient, jobHttpClient, dbContext);
        Result<Domain.Worker> result = await zarf.RegisterAsync(context, stoppingToken);
        if (result.DomainFailed)
        {
            return result.DomainFailureResult;
        }
        
        Domain.Worker? worker = result.OkResult.Value;
        
        await zarf.StartHeartBeatAsync(context, worker, stoppingToken);
        await zarf.StartDoingJobAsync(context, worker, stoppingToken);
        return zarf;
    }


    private async Task<Result<Domain.Worker>> RegisterAsync(WorkerContext context, CancellationToken stoppingToken)
    {
        while (context.MasterUnavailable)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }

        return await workerService.RegisterAsync(stoppingToken);
    }

    private async Task StartHeartBeatAsync(WorkerContext context, Domain.Worker worker, CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (context.MasterUnavailable)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                continue;
            }

            if (worker.ShouldNotReportHeartBeat)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                continue;
            }

            if (await workerHttpClient.HeartBeat(worker.Id))
            {
                worker.ReportHeartBeat();
                logger.LogInformation("Heartbeat complete at {time}", DateTimeOffset.Now);
                await dbContext.SaveChangesAsync(stoppingToken);
            }
        }
    }

    private async Task StartDoingJobAsync(WorkerContext context, Domain.Worker worker, CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await AssignJobAsync(context, worker, stoppingToken);
            await ProcessJobAsync(context, worker, stoppingToken);
            await FinishJobAsync(context, worker, stoppingToken);
        }
    }

    private async Task AssignJobAsync(WorkerContext context, Domain.Worker worker, CancellationToken stoppingToken)
    {
        while (context.MasterUnavailable)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }

        _ = await workerService.AssignJobAsync(worker.Id, stoppingToken);
        
    }

    private async Task ProcessJobAsync(WorkerContext context, Domain.Worker worker, CancellationToken stoppingToken)
    {
        while (context.MasterUnavailable)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
        
        _ = await workerService.ProcessJobAsync(worker.Id, stoppingToken);
    }
    
    private async Task FinishJobAsync(WorkerContext context, Domain.Worker worker, CancellationToken stoppingToken)
    {
        while (context.MasterUnavailable)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
        
        _ = await workerService.FinishJobAsync(worker.Id, stoppingToken);
    }

    
    
}