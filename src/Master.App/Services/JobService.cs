using Master.Domain.Aggregates;
using Master.Domain.Models;
using Master.Domain.Repositories;
using Master.Domain.Services;
using Shared.Domain.Failures;

namespace Master.App.Services;

public class JobService(IJobRepository jobRepository, IWorkerRepository workerRepository) : IJobService
{
    public List<Job> Jobs => jobRepository.Jobs;

    public async Task<(IError?, Job?)> QueueJob(string name)
    {
        Job job = new Job(name);
        IError? error = await jobRepository.AddAsync(job);
        if (error is not null)
        {
            return new(error, null);
        }
        error = await jobRepository.UnitOfWork.SaveEntitiesAsync();
        if (error is not null)
        {
            return new(error, null);
        }
        return new(null, job);
    }

    public async Task<(IError?, Job?)> AssignJob(Guid workerId)
    {
        (IError? error, Job? queuedJob) = await jobRepository.DequeueAsync();
        if (error is not null)
        {
            return new(error, null);
        }

        (error, Worker? worker) = await workerRepository.GetAsync(workerId);
        if (error is not null)
        {
            return new(error, null);
        }
        (error, Job? assignedJob) = queuedJob!.Assign(workerId);
        if (error is not null)
        {
            return new(error, null);
        }
        error = await jobRepository.UnitOfWork.SaveEntitiesAsync();
        if (error is not null)
        {
            return new(error, null);
        }
        return new(null, assignedJob);
    }

    public async Task<(IError?, Job?)> StartJob(Guid jobId, Guid workerId)
    {
        (IError? error, Job? job) = await jobRepository.GetAsync(jobId);
        if (error is not null)
        {
            return new(error, null);
        }
        (error, Job? runningJob) = job.TryStart(workerId);
        if (error is not null)
        {
            return new(error, null);
        }
        error = await jobRepository.UnitOfWork.SaveEntitiesAsync();
        if (error is not null)
        {
            return new(error, null);
        }
        return new(null, runningJob);
    }

    public async Task<(IError?, Job?)> CompleteJob(Guid jobId, Guid workerId)
    {
        (IError? error, Job? job) = await jobRepository.GetAsync(jobId);
        if (error is not null)
        {
            return new(error, null);
        }

        (error, Job? completedJob) = job.TryComplete(workerId);
        if (error is not null)
        {
            return new(error, null);
        }
        error = await jobRepository.UnitOfWork.SaveEntitiesAsync();
        if (error is not null)
        {
            return new(error, null);
        }
        return new(null, completedJob);
    }

    public async Task<(IError?, Job?)> FailJob(Guid jobId, Guid workerId)
    {
        (var error, Job? job) = await jobRepository.GetAsync(jobId);
        if (error is not null)
        {
            return new(error, null);
        }

        (error, Job? failedJob) = job.TryFail(workerId);
        if (error is not null)
        {
            return new(error, null);
        }
        error = await jobRepository.UnitOfWork.SaveEntitiesAsync();
        if (error is not null)
        {
            return new(error, null);
        }

        return new(null, failedJob);
    }
}