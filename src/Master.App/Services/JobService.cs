using Master.Domain.Aggregates;
using Master.Domain.Models;
using Master.Domain.Repositories;
using Master.Domain.Services;
using Shared.Domain.DTOs;

namespace Master.App.Services;

public class JobService(IJobRepository jobRepository, IWorkerRepository workerRepository) : IJobService
{
    public async Task<List<Job>> AllAsync() => await jobRepository.AllAsync();

    public async Task<IResult<Job>> QueueJobAsync(string name)
    {
        Job job = new Job(name);
        await jobRepository.AddAsync(job);
        
        IResult result = await jobRepository.UnitOfWork.SaveEntitiesAsync();
        if (result is CriticalError criticalError)
        {
            return new CriticalError<Job>(criticalError.Message);
        }

        return new Ok<Job>(job);
    }
    
    
    private async Task<Result<Job>> ExecuteJobCommand1(
        Guid workerId,
        Func<Task<IResult<Job>>> loadJob,
        Func<Job, Guid, IResult> jobStateTransition
    )
    {
        IResult result = await workerRepository.GetAsync(workerId);
        if (result is NotFound<Worker> workerNotFound)
        {
            return new NotFound(workerNotFound.Message);
        }
     
        result = await loadJob();
        if (result is NotFound<Job> jobNotFound)
        {
            return jobNotFound;
        }
        Ok<Job> ok = result as Ok<Job> ?? throw new InvalidOperationException();
        if (result is Job job)
        {
            result = jobStateTransition(job, workerId);    
        }
        // Job job = ok.Value;
        

        if (result is DomainFailure error)
        {
            return new Error<Job>(error.Message);
        }

        result = await jobRepository.UnitOfWork.SaveEntitiesAsync();

        if (result is CriticalError criticalError)
        {
            return new CriticalError<Job>(criticalError.Message);
        }

        return new Ok<Job>(job);
    }

    
    private async Task<IResult<Job>> ExecuteJobCommand(
        Guid workerId,
        Func<Task<IResult<Job>>> loadJob,
        Func<Job, Guid, IResult> jobStateTransition
        )
    {
        IResult result = await workerRepository.GetAsync(workerId);
        if (result is NotFound<Worker> workerNotFound)
        {
            return new NotFound<Job>(workerNotFound.Message);
        }
     
        result = await loadJob();
        if (result is NotFound<Job> jobNotFound)
        {
            return jobNotFound;
        }
        Ok<Job> ok = result as Ok<Job> ?? throw new InvalidOperationException();
        Job job = ok.Value;
        result = jobStateTransition(job, workerId);

        if (result is DomainFailure error)
        {
            return new Error<Job>(error.Message);
        }

        result = await jobRepository.UnitOfWork.SaveEntitiesAsync();

        if (result is CriticalError criticalError)
        {
            return new CriticalError<Job>(criticalError.Message);
        }

        return new Ok<Job>(job);
    }
    

    public async Task<IResult<Job>> AssignJobAsync(Guid workerId) =>
        await ExecuteJobCommand(workerId, 
            async () => await jobRepository.GetQueuedJobAsync(),
            (job, id) => job.Assign(workerId));

    public async Task<IResult<Job>> StartJobAsync(Guid jobId, Guid workerId) =>
        await ExecuteJobCommand(workerId, 
            async () => await jobRepository.GetQueuedJobAsync(),
            (job, id) => job.Start(workerId));

    public async Task<IResult<Job>> CompleteJobAsync(Guid jobId, Guid workerId) =>
        await ExecuteJobCommand(workerId, 
            async () => await jobRepository.GetQueuedJobAsync(),
            (job, id) => job.Complete(workerId));

    public async Task<IResult<Job>> FailJobAsync(Guid jobId, Guid workerId) =>
        await ExecuteJobCommand(workerId, 
            async () => await jobRepository.GetQueuedJobAsync(),
            (job, id) => job.Fail(workerId));
}