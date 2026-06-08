using Bogus;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.DTOs;
using Shared.Domain.Models;
using Worker.Rest.Contexts;
using Worker.Rest.EF;
using Worker.Rest.HttpServices.Master;
using Worker.Rest.Works;
using IResult = Shared.Domain.DTOs.IResult;

namespace Worker.Rest.Services;

public class Zarf(
    ILogger<Zarf> logger,
    IWorkerHttpClient workerHttpClient,
    IJobHttpClient jobHttpClient,
    IDbContextFactory<WorkerDbContext> factory)
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkerId { get; set; } = Guid.Empty;

    public static async Task<Result<Zarf>> RegisterAsync(ILogger<Zarf> logger,
        IWorkerHttpClient workerHttpClient,
        IJobHttpClient jobHttpClient, IDbContextFactory<WorkerDbContext> factory,
        WorkerContext context, CancellationToken stoppingToken)
    {
        Zarf zarf = new(logger, workerHttpClient, jobHttpClient, factory);
        Result<Domain.Worker> result = await zarf.RegisterAsync(context, stoppingToken);
        if (result.WrappedResult is DomainFailure failure)
        {
            return failure;
        }

        if (result.TryGetValue(out Domain.Worker? worker))
        {
            await zarf.StartHeartBeatAsync(context, worker, stoppingToken);
            await zarf.StartDoingJobAsync(context, worker, stoppingToken);    
        }
        
        return zarf;
    }


    private async Task<Result<Domain.Worker>> RegisterAsync(WorkerContext context, CancellationToken stoppingToken)
    {
        while (context.MasterUnavailable)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }

        Faker faker = new Faker();
        string name = faker.Company.CompanyName()
            .ToLower()
            .Replace(" ", "-");

        Result<Domain.Worker> result = await workerHttpClient.Register(string.Empty);
        if (result.WrappedResult is Ok<Domain.Worker> ok)
        {
            Domain.Worker worker = ok.Value;
            await using WorkerDbContext db = await factory.CreateDbContextAsync(stoppingToken);
            worker.Register();
            await db.Workers.AddAsync(worker, stoppingToken);
            await db.SaveChangesAsync(stoppingToken);
            WorkerId = worker.Id;
            logger.LogInformation($"Registered {name} (worker id: {worker.Id})");
            return worker;
        }

        logger.LogError($"Failed to register {name}");
        return new DomainFailure($"Failed to register {name}");
    }

    private async Task StartHeartBeatAsync(WorkerContext context, Domain.Worker worker, CancellationToken stoppingToken)
    {
        await using WorkerDbContext db = await factory.CreateDbContextAsync(stoppingToken);
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
                await db.SaveChangesAsync(stoppingToken);
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

        await using WorkerDbContext dbContext = await factory.CreateDbContextAsync(stoppingToken);

        if (worker.IsJobAssigned)
        {
            logger.LogWarning("Worker already assigned.");
            return;
        }

        (IContentMessage? error, Job? job) = await jobHttpClient.GetJobAsync(worker.Id, stoppingToken);
        if (error != null)
        {
            logger.LogError(error.ToString());
            return;
        }

        if (job == null)
        {
            logger.LogError("Instance of job is null");
            return ;
        }

        worker.AssignJob(job.Id);
        await dbContext.SaveChangesAsync(stoppingToken);
    }

    private async Task ProcessJobAsync(WorkerContext context, Domain.Worker worker, CancellationToken stoppingToken)
    {
        if (!worker.ReadyToProcessJob)
        {
            logger.LogError("Instance of job is not ready");
            return;
        }
        await using WorkerDbContext dbContext = await factory.CreateDbContextAsync(stoppingToken);

        var (error, job) = await jobHttpClient.StartJobAsync(worker.Id, worker.JobId, stoppingToken);

        if (error != null)
        {
            logger.LogError(error.ToString());
            return;
        }

        if (job == null)
        {
            logger.LogError("Instance of job is null");
            return;
        }

        await SimpleWork.Run(stoppingToken);
        
        worker.StartJob(job.Id);
        await dbContext.SaveChangesAsync(stoppingToken);
    }
    
    private async Task FinishJobAsync(WorkerContext context, Domain.Worker worker, CancellationToken stoppingToken)
    {
        await using WorkerDbContext dbContext = await factory.CreateDbContextAsync(stoppingToken);
        JobCompletionRequest request = new(worker.Id, worker.JobId);
        var (error, job) = await jobHttpClient.ResultJobAsync(request, stoppingToken);

        if (error != null)
        {
            logger.LogError(error.ToString());
            return;
        }

        if (job == null)
        {
            logger.LogError("Instance of job is null");
            return;
        }

        worker.CompleteJob();
        await dbContext.SaveChangesAsync(stoppingToken);
    }

    
    
}