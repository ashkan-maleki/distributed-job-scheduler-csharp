using Bogus;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.DTOs;
using Shared.Domain.Models;
using Worker.Rest.Contexts;
using Worker.Rest.EF;
using Worker.Rest.HttpServices.Master;
using Worker.Rest.Works;

namespace Worker.Rest.Services;

public class Zarf(
    ILogger<Zarf> logger,
    Domain.Worker worker,
    IWorkerHttpClient workerHttpClient,
    IJobHttpClient jobHttpClient,
    IDbContextFactory<WorkerDbContext> factory)
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? WorkerId { get; set; }

    public static async Task<Zarf> RegisterAsync(ILogger<Zarf> logger, Domain.Worker worker,
        IWorkerHttpClient workerHttpClient,
        IJobHttpClient jobHttpClient, IDbContextFactory<WorkerDbContext> factory,
        WorkerContext context, CancellationToken stoppingToken)
    {
        Zarf zarf = new(logger, worker, workerHttpClient, jobHttpClient, factory);
        await zarf.RegisterAsync(context, stoppingToken);
        await zarf.StartHeartBeatAsync(context, stoppingToken);
        await zarf.StartDoingJobAsync(context, stoppingToken);
        return zarf;
    }


    private async Task RegisterAsync(WorkerContext context, CancellationToken stoppingToken)
    {
        while (context.MasterUnavailable)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }

        Faker faker = new Faker();
        string name = faker.Company.CompanyName()
            .ToLower()
            .Replace(" ", "-");

        Result<Domain.Worker> result = await workerHttpClient.Register(name);
        if (result.WrappedResult is Ok<Domain.Worker> ok)
        {
            Domain.Worker worker = ok.Value;
            await using WorkerDbContext db = await factory.CreateDbContextAsync(stoppingToken);
            worker.Register();
            await db.Workers.AddAsync(worker, stoppingToken);
            await db.SaveChangesAsync(stoppingToken);
            logger.LogInformation($"Registered {name} (worker id: {worker.Id})");
        }
        else
        {
            logger.LogError($"Failed to register {name}");
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }

    private async Task StartHeartBeatAsync(WorkerContext context, CancellationToken stoppingToken)
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
            }
        }
    }

    private async Task StartDoingJobAsync(WorkerContext context, CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            IContentMessage? error;
            var (dbContext, worker, job) = await AssignJob(context, stoppingToken);
            await using var workerDbContext = dbContext;

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

    private async Task<(WorkerDbContext dbContext, Domain.Worker? worker, Job? job)> AssignJob(WorkerContext context,
        CancellationToken stoppingToken)
    {
        while (context.MasterUnavailable)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }

        await using WorkerDbContext? dbContext = await factory.CreateDbContextAsync(stoppingToken);
        ;

        Domain.Worker? worker = await dbContext.Workers.FindAsync(WorkerId, stoppingToken);

        if (worker == null)
        {
            logger.LogWarning("Worker not found.");
            return (dbContext, worker, null);
        }

        if (worker.JobId is not null)
        {
            logger.LogWarning("Worker already assigned.");
            return (dbContext, worker, null);
        }

        (IContentMessage? error, Job? job) = await jobHttpClient.GetJobAsync(worker.Id, stoppingToken);
        if (error != null)
        {
            logger.LogError(error.ToString());
            return (dbContext, worker, job);
        }

        if (job == null)
        {
            logger.LogError("Instance of job is null");
            return (dbContext, worker, job);
        }

        worker.AssignJob(job.Id);
        await dbContext.SaveChangesAsync(stoppingToken);
        return (dbContext, worker, job);
    }
}