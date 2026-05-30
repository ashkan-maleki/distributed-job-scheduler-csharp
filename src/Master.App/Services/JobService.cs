using Master.Domain.Aggregates;
using Master.Domain.Models;
using Master.Domain.Repositories;
using Master.Domain.Services;
using Shared.Domain.DTOs;

namespace Master.App.Services;

public class JobService(IJobRepository jobRepository, IWorkerRepository workerRepository) : IJobService
{
    public async Task<List<Job>> AllAsync() => await jobRepository.AllAsync();

    public async Task<IResult<Job>> QueueJob(string name)
    {
        Job job = new Job(name);
        await jobRepository.AddAsync(job);
        
        Exception? exception = await jobRepository.UnitOfWork.SaveEntitiesAsync();
        if (exception is not null)
        {
            return QueryResults.ExceptionThrown<Job>(exception);
        }

        return new Ok<Job>(job);
    }

    public async Task<IResult<Job>> AssignJob(Guid workerId)
    {
        QueryResult2<Job> jobQueryResult2 = await jobRepository.GetQueuedJobAsync();
        if (jobQueryResult2.NotFound)
        {
            return jobQueryResult2.ToQueryResult<Job>();
        }

        QueryResult2<Worker> workerQueryResult2 = await workerRepository.GetAsync(workerId);
        if (workerQueryResult2.NotFound)
        {
            return  workerQueryResult2.SwapPayload<Job>();
        }

        Job job = jobQueryResult2.Data;
        IResult result = job.Assign(workerId);
        if (result is Error error)
        {
            return QueryResults.DomainFailure<Job>(error.Content);
        }
        Exception? exception = await jobRepository.UnitOfWork.SaveEntitiesAsync();
        if (exception is not null)
        {
            return QueryResults.ExceptionThrown<Job>(exception);
        }
        return QueryResults.Found(job);
    }

    public async Task<IResult<Job>> StartJob(Guid jobId, Guid workerId)
    {
        QueryResult2<Job> jobQueryResult2 = await jobRepository.GetAsync(jobId);
        if (jobQueryResult2.NotFound)
        {
            return jobQueryResult2.ToQueryResult<Job>();
        }
        
        Job job = jobQueryResult2.Data;
        IResult result = job.Start(workerId);
        if (result is Error error)
        {
            return result.ToQueryResult<Job>();
        }
        Exception? exception = await jobRepository.UnitOfWork.SaveEntitiesAsync();
        if (exception is not null)
        {
            return QueryResults.ExceptionThrown<Job>(exception);
        }
        return QueryResults.Found(job);
    }

    public async Task<IResult<Job>> CompleteJob(Guid jobId, Guid workerId)
    {
        QueryResult2<Job> jobQueryResult2 = await jobRepository.GetAsync(jobId);
        if (jobQueryResult2.NotFound)
        {
            return jobQueryResult2.ToQueryResult<Job>();
        }
        
        Job job = jobQueryResult2.Data;
        IResult result = job.Complete(workerId);
        if (result is Error error)
        {
            return result2.ToQueryResult<Job>();
        }
        
        Exception? exception = await jobRepository.UnitOfWork.SaveEntitiesAsync();
        if (exception is not null)
        {
            return QueryResults.ExceptionThrown<Job>(exception);
        }
        return QueryResults.Found(job);
    }

    public async Task<IResult<Job>> FailJob(Guid jobId, Guid workerId)
    {
        QueryResult2<Job> jobQueryResult2 = await jobRepository.GetAsync(jobId);
        if (jobQueryResult2.NotFound)
        {
            return jobQueryResult2.ToQueryResult<Job>();
        }
        
        Job job = jobQueryResult2.Data;
        IResult result = job.Fail(workerId);
        if (result is Error error)
        {
            return result2.ToQueryResult<Job>();
        }
        
        Exception? exception = await jobRepository.UnitOfWork.SaveEntitiesAsync();
        if (exception is not null)
        {
            return QueryResults.ExceptionThrown<Job>(exception);
        }
        return QueryResults.Found(job);
    }

}