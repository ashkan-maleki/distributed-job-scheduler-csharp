using Master.Domain.Aggregates;
using Master.Domain.Models;
using Master.Domain.Repositories;
using Master.Domain.Services;
using Shared.Domain.Failures;

namespace Master.App.Services;

public class JobService(IJobRepository jobRepository, IWorkerRepository workerRepository) : IJobService
{
    public async Task<List<Job>> AllAsync() => await jobRepository.AllAsync();

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
        (IError? error, Job? job) = await jobRepository.DequeueAsync();
        if (error is not null)
        {
            return new(error, null);
        }

        (error, Worker? worker) = await workerRepository.GetAsync(workerId);
        if (error is not null)
        {
            return new(error, null);
        }
        error = job!.Assign(workerId);
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

    public async Task<(IError?, Job?)> StartJob(Guid jobId, Guid workerId)
    {
        (IError? error, Job? job) = await jobRepository.GetAsync(jobId);
        if (error is not null)
        {
            return new(error, null);
        }
        error = job!.Start(workerId);
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

    public async Task<(IError?, Job?)> CompleteJob(Guid jobId, Guid workerId)
    {
        (IError? error, Job? job) = await jobRepository.GetAsync(jobId);
        if (error is not null)
        {
            return new(error, null);
        }

        error = job!.Complete(workerId);
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

    public async Task<(IError?, Job?)> FailJob(Guid jobId, Guid workerId)
    {
        (var error, Job? job) = await jobRepository.GetAsync(jobId);
        if (error is not null)
        {
            return new(error, null);
        }

        error = job!.Fail(workerId);
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

}