using Master.Domain.Aggregates;
using Master.Domain.Models;
using Master.Domain.Repositories;
using Master.Domain.Services;
using Shared.Domain.DTOs;

namespace Master.App.Services;

public class JobService(IJobRepository jobRepository, IWorkerRepository workerRepository
    , IDesiredStateService desiredStateService) : IJobService
{
    public async Task<List<Job>> AllAsync() => await jobRepository.AllAsync();

    public async Task<Result<Job>> QueueJobAsync(string name)
    {
        Job job = new Job(name);
        await jobRepository.AddAsync(job);

        IResult result = await jobRepository.UnitOfWork.SaveEntitiesAsync();
        if (result is CriticalError criticalError)
        {
            return criticalError;
        }

        return job;
    }


    private async Task<Result<Job>> ExecuteJobCommand(
        Guid workerId,
        Func<Task<Result<Job>>> loadJob,
        Func<Job, Guid, IResult> jobStateTransition
    )
    {
        Result<Worker> workerResult = await workerRepository.GetAsync(workerId);
        if (workerResult.NotFound)
        {
            return workerResult.NotFoundResult;
        }

        Result<Job> jobResult = await loadJob();
        if (jobResult.NotFound)
        {
            return jobResult.NotFoundResult;
        }
        
        Job job = jobResult.OkResult.Value;
        IResult result = jobStateTransition(job, workerId);
        if (result is DomainFailure error)
        {
            return error;
        }

        result = await jobRepository.UnitOfWork.SaveEntitiesAsync();

        if (result is CriticalError criticalError)
        {
            return criticalError;
        }

        return job;
    }


    public async Task<Result<Job>> AssignJobAsync(Guid workerId) =>
        await ExecuteJobCommand(workerId,
            async () => await jobRepository.GetQueuedJobAsync(),
            (job, _) => job.Assign(workerId));

    public async Task<Result<Job>> StartJobAsync(Guid jobId, Guid workerId) =>
        await ExecuteJobCommand(workerId,
            async () => await jobRepository.GetAsync(jobId),
            (job, _) => job.Start(workerId));

    
    
    private async Task<Result<Job>> ExecuteJobCompletionCommand(
        Guid workerId,
        Func<Task<Result<Job>>> loadJob,
        Func<Job, Guid, IResult> jobStateTransition
    )
    {
        Result<Worker> workerResult = await workerRepository.GetAsync(workerId);
        if (workerResult.NotFound)
        {
            return workerResult.NotFoundResult;
        }

        Result<Job> jobResult = await loadJob();
        if (jobResult.NotFound)
        {
            return jobResult.NotFoundResult;
        }
        
        Job job = jobResult.OkResult.Value;
        IResult result = jobStateTransition(job, workerId);
        if (result is DomainFailure error)
        {
            return error;
        }

        if (await desiredStateService.DesiredNumberOfWorkersAsync() < await workerRepository.CountAsync())
        {
            workerRepository.Remove(workerResult.OkResult.Value);
        }

        result = await jobRepository.UnitOfWork.SaveEntitiesAsync();

        if (result is CriticalError criticalError)
        {
            return criticalError;
        }

        return job;
    }
    
    public async Task<Result<Job>> CompleteJobAsync(Guid jobId, Guid workerId) =>
        await ExecuteJobCompletionCommand(workerId,
            async () => await jobRepository.GetAsync(jobId),
            (job, _) => job.Complete(workerId));

    public async Task<Result<Job>> FailJobAsync(Guid jobId, Guid workerId) =>
        await ExecuteJobCompletionCommand(workerId,
            async () => await jobRepository.GetAsync(jobId),
            (job, _) => job.Fail(workerId));
}
