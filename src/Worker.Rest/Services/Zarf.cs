using Microsoft.EntityFrameworkCore;
using Shared.Domain.DTOs;
using Worker.Rest.Contexts;
using Worker.Rest.EF;
using Worker.Rest.HttpServices.Master;

namespace Worker.Rest.Services;

public class Zarf(
    ILogger<Zarf> logger,
    IWorkerService workerService,
    IJobService jobService,
    IMasterHealthCheckService  masterHealthCheckService,
    IWorkerHttpClient workerHttpClient,
    WorkerDbContext dbContext)
{
    public static async Task<Result<Zarf>> RegisterAsync(ILogger<Zarf> logger,
        IWorkerService workerService,
        IJobService jobService,
        IMasterHealthCheckService  masterHealthCheckService,
        IWorkerHttpClient workerHttpClient,
        IDbContextFactory<WorkerDbContext> factory,
        CancellationToken stoppingToken)
    {
        await using WorkerDbContext dbContext = await factory.CreateDbContextAsync(stoppingToken);
        Zarf zarf = new(logger, workerService, jobService, masterHealthCheckService, workerHttpClient, dbContext);
        Result<Domain.Worker> result = await zarf.RegisterAsync(stoppingToken);
        if (result.DomainFailed)
        {
            return result.DomainFailureResult;
        }
        
        Domain.Worker worker = result.Value;
        
        await zarf.StartHeartBeatAsync(worker, stoppingToken);
        await zarf.StartDoingJobAsync(worker, stoppingToken);
        return zarf;
    }


    private async Task<Result<Domain.Worker>> RegisterAsync(CancellationToken stoppingToken)
    {
        await masterHealthCheckService.IsMasterAvailableAsync(stoppingToken);
        return await workerService.RegisterAsync(stoppingToken);
    }

    private async Task StartHeartBeatAsync(Domain.Worker worker, CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await masterHealthCheckService.IsMasterAvailableAsync(stoppingToken);

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

    private async Task StartDoingJobAsync(Domain.Worker worker, CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await AssignJobAsync(worker, stoppingToken);
            await ProcessJobAsync(worker, stoppingToken);
            await FinishJobAsync(worker, stoppingToken);
        }
    }

    private async Task AssignJobAsync( Domain.Worker worker, CancellationToken stoppingToken)
    {
        await masterHealthCheckService.IsMasterAvailableAsync(stoppingToken);

        _ = await jobService.AssignJobAsync(worker.Id, stoppingToken);
        
    }

    private async Task ProcessJobAsync( Domain.Worker worker, CancellationToken stoppingToken)
    {
        await masterHealthCheckService.IsMasterAvailableAsync(stoppingToken);
        
        _ = await jobService.ProcessJobAsync(worker.Id, stoppingToken);
    }
    
    private async Task FinishJobAsync(Domain.Worker worker, CancellationToken stoppingToken)
    {
        await masterHealthCheckService.IsMasterAvailableAsync(stoppingToken);
        
        _ = await jobService.FinishJobAsync(worker.Id, stoppingToken);
    }

    
    
}