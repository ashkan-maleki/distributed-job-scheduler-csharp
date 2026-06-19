using Shared.Domain.DTOs;
using Shared.Domain.Models;
using Worker.Rest.HttpServices.Master;
using Worker.Rest.Stores;
using Worker.Rest.Works;
using IResult = Shared.Domain.DTOs.IResult;

namespace Worker.Rest.Services;


public class WorkerService(IWorkerStore store, IWorkerHttpClient workHttpClient,
    IJobHttpClient jobHttpClient, ILogger<WorkerService> logger) : IWorkerService
{
    public async Task<Result<Domain.Worker>> RegisterAsync(CancellationToken stoppingToken)
    {
        Result<Domain.Worker> result = await workHttpClient.Register();
        if (result.DomainFailed)
        {
            return new DomainFailure($"Failed to register: " + result.DomainFailureResult.Message);
        }
        
        Domain.Worker worker = result.OkResult.Value;
        
        worker.Register();
        await store.AddAsync(worker, stoppingToken);
        IResult saveResult = await store.UnitOfWork.SaveEntitiesAsync(stoppingToken);
        if (saveResult is CriticalError error)
        {
            return error;
        }

        logger.LogInformation($"Registered the worker with id: {worker.Id}, named: {worker.Name}");
        return worker;
    }

    public async Task<Result<Domain.Worker>> AssignJobAsync(Guid workerId, CancellationToken stoppingToken)
    {
        Result<Domain.Worker> result = await store.FindAsync(workerId, stoppingToken);
        if (result.NotFound)
        {
            return result.NotFoundResult;
        }
        
        Domain.Worker worker = result.Value;
        if (worker.HasJobAssigned)
        {
            return new DomainFailure("Worker already assigned.");
        }

        Result<Job> jobResult = await jobHttpClient.GetJobAsync(worker.Id, stoppingToken);
        if (jobResult.DomainFailed)
        {
            return jobResult.DomainFailureResult;
        }

        Job job = jobResult.Value;
        worker.AssignJob(job.Id);
        IResult saveResult = await store.UnitOfWork.SaveEntitiesAsync(stoppingToken);
        if (saveResult is CriticalError error)
        {
            return error;
        }
        logger.LogInformation($"Assigned a job ({job.Id}) the worker with id: {worker.Id}, named: {worker.Name}");
        return worker;
    }

    public async Task<Result<Domain.Worker>> ProcessJobAsync(Guid workerId, CancellationToken stoppingToken)
    {
        Result<Domain.Worker> result = await store.FindAsync(workerId, stoppingToken);
        if (result.NotFound)
        {
            return result.NotFoundResult;
        }
        Domain.Worker worker = result.Value;
        if (worker.NotReadyToProcessJob)
        {
            return new DomainFailure("Worker has not assigned a job.");
        }
        

        Result<Job> jobResult = await jobHttpClient.StartJobAsync(worker.Id, worker.JobId, stoppingToken);
        if (jobResult.DomainFailed)
        {
            return jobResult.DomainFailureResult;
        }

        Job job = jobResult.Value;
        await SimpleWork.Run(stoppingToken);
        
        worker.StartJob(job.Id);
        IResult saveResult = await store.UnitOfWork.SaveEntitiesAsync(stoppingToken);
        if (saveResult is CriticalError error)
        {
            return error;
        }
        logger.LogInformation($"Started the job ({job.Id}) for the worker with id: {worker.Id}, named: {worker.Name}");
        return worker;
    }

    public async Task<Result<Domain.Worker>> FinishJobAsync(Guid workerId, CancellationToken stoppingToken)
    {
        Result<Domain.Worker> result = await store.FindAsync(workerId, stoppingToken);
        if (result.NotFound)
        {
            return result.NotFoundResult;
        }
        Domain.Worker worker = result.Value;
        JobCompletionRequest request = new(worker.Id, worker.JobId);
        Result<Job> jobResult = await jobHttpClient.ResultJobAsync(request, stoppingToken);
        if (jobResult.DomainFailed)
        {
            return jobResult.DomainFailureResult;
        }

        worker.CompleteJob();
        IResult saveResult = await store.UnitOfWork.SaveEntitiesAsync(stoppingToken);
        if (saveResult is CriticalError error)
        {
            return error;
        }
        
        return worker;
    }
}

public interface IWorkerService
{
    Task<Result<Domain.Worker>> RegisterAsync(CancellationToken stoppingToken);
    Task<Result<Domain.Worker>> AssignJobAsync(Guid workerId, CancellationToken stoppingToken);
    Task<Result<Domain.Worker>> ProcessJobAsync(Guid workerId, CancellationToken stoppingToken);
    Task<Result<Domain.Worker>> FinishJobAsync(Guid workerId, CancellationToken stoppingToken);
}