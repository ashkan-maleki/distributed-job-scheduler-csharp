using Master.Domain.Aggregates;
using Master.Domain.Models;
using Master.Domain.Repositories;
using Master.Domain.Services;
using Shared.Domain.DTOs;

namespace Master.App.Services;

public class JobService(IJobRepository jobRepository, IWorkerRepository workerRepository) : IJobService
{
    public async Task<List<Job>> AllAsync() => await jobRepository.AllAsync();

    public async Task<Result<Job>> QueueJobAsync(string name)
    {
        Job job = new Job(name);
        await jobRepository.AddAsync(job);

        IResult result = await jobRepository.UnitOfWork.SaveEntitiesAsync();
        if (result is CriticalError criticalError)
        {
            return new CriticalError(criticalError.Message);
        }

        return job;
    }


    private async Task<Result<Job>> ExecuteJobCommand(
        Guid workerId,
        Func<Task<Result<Job>>> loadJob,
        Func<Job, Guid, IResult> jobStateTransition
    )
    {
        IResult result = await workerRepository.GetAsync(workerId);
        if (result is NotFound workerNotFound)
        {
            return workerNotFound;
        }

        result = await loadJob();
        if (result is not Object<Job> jobObject)
        {
            return (NotFound)result;
        }
        
        Job job = jobObject.Value;
        result = jobStateTransition(job, workerId);
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
            (job, id) => job.Assign(workerId));

    public async Task<Result<Job>> StartJobAsync(Guid jobId, Guid workerId) =>
        await ExecuteJobCommand(workerId,
            async () => await jobRepository.GetQueuedJobAsync(),
            (job, id) => job.Start(workerId));

    public async Task<Result<Job>> CompleteJobAsync(Guid jobId, Guid workerId) =>
        await ExecuteJobCommand(workerId,
            async () => await jobRepository.GetQueuedJobAsync(),
            (job, id) => job.Complete(workerId));

    public async Task<Result<Job>> FailJobAsync(Guid jobId, Guid workerId) =>
        await ExecuteJobCommand(workerId,
            async () => await jobRepository.GetQueuedJobAsync(),
            (job, id) => job.Fail(workerId));
}

// private async Task<IResult<Job>> ExecuteJobCommand(
//     Guid workerId,
//     Func<Task<IResult<Job>>> loadJob,
//     Func<Job, Guid, IResult> jobStateTransition
// )
// {
//     IResult result = await workerRepository.GetAsync(workerId);
//     if (result is NotFound<Worker> workerNotFound)
//     {
//         return new NotFound<Job>(workerNotFound.Message);
//     }
//
//     result = await loadJob();
//     if (result is NotFound<Job> jobNotFound)
//     {
//         return jobNotFound;
//     }
//
//     Ok<Job> ok = result as Ok<Job> ?? throw new InvalidOperationException();
//     Job job = ok.Value;
//     result = jobStateTransition(job, workerId);
//
//     if (result is DomainFailure error)
//     {
//         return new Error<Job>(error.Message);
//     }
//
//     result = await jobRepository.UnitOfWork.SaveEntitiesAsync();
//
//     if (result is CriticalError criticalError)
//     {
//         return new CriticalError<Job>(criticalError.Message);
//     }
//
//     return new Ok<Job>(job);
// }