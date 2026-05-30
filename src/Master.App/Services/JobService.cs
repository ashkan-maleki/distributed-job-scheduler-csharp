using Master.Domain.Aggregates;
using Master.Domain.Models;
using Master.Domain.Repositories;
using Master.Domain.Services;
using Shared.Domain.DTOs;

namespace Master.App.Services;

public class JobService(IJobRepository jobRepository, IWorkerRepository workerRepository) : IJobService
{
    public async Task<List<Job>> AllAsync() => await jobRepository.AllAsync();

    public async Task<QueryResult2<Job>> QueueJob(string name)
    {
        Job job = new Job(name);
        Result2 result2 = await jobRepository.AddAsync(job);
        if (!result2.Ok)
        {
            return result2.ToQueryResult<Job>();
        }
        Exception? exception = await jobRepository.UnitOfWork.SaveEntitiesAsync();
        if (exception is not null)
        {
            return QueryResults.ExceptionThrown<Job>(exception);
        }
        return QueryResults.Ok<Job>();
    }

    public async Task<QueryResult2<Job>> AssignJob(Guid workerId)
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

    public async Task<QueryResult2<Job>> StartJob(Guid jobId, Guid workerId)
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

    public async Task<QueryResult2<Job>> CompleteJob(Guid jobId, Guid workerId)
    {
        QueryResult2<Job> jobQueryResult2 = await jobRepository.GetAsync(jobId);
        if (jobQueryResult2.NotFound)
        {
            return jobQueryResult2.ToQueryResult<Job>();
        }
        
        Job job = jobQueryResult2.Data;
        Result2 result2 = job.Complete(workerId);
        if (!result2.Ok)
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

    public async Task<QueryResult2<Job>> FailJob(Guid jobId, Guid workerId)
    {
        QueryResult2<Job> jobQueryResult2 = await jobRepository.GetAsync(jobId);
        if (jobQueryResult2.NotFound)
        {
            return jobQueryResult2.ToQueryResult<Job>();
        }
        
        Job job = jobQueryResult2.Data;
        Result2 result2 = job.Fail(workerId);
        if (!result2.Ok)
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