using Master.Domain.Aggregates;
using Master.Domain.Models;
using Master.Domain.Repositories;
using Master.Domain.Services;
using Shared.Domain.DTOs;

namespace Master.App.Services;

public class JobService(IJobRepository jobRepository, IWorkerRepository workerRepository) : IJobService
{
    public async Task<List<Job>> AllAsync() => await jobRepository.AllAsync();

    public async Task<QueryResult<Job>> QueueJob(string name)
    {
        Job job = new Job(name);
        Result result = await jobRepository.AddAsync(job);
        if (!result.Ok)
        {
            return result.ToQueryResult<Job>();
        }
        Exception? exception = await jobRepository.UnitOfWork.SaveEntitiesAsync();
        if (exception is not null)
        {
            return QueryResults.ExceptionThrown<Job>(exception);
        }
        return QueryResults.Ok<Job>();
    }

    public async Task<QueryResult<Job>> AssignJob(Guid workerId)
    {
        QueryResult<Job> jobQueryResult = await jobRepository.GetQueuedJobAsync();
        if (jobQueryResult.NotFound)
        {
            return jobQueryResult.ToQueryResult<Job>();
        }

        QueryResult<Worker> workerQueryResult = await workerRepository.GetAsync(workerId);
        if (workerQueryResult.NotFound)
        {
            return  workerQueryResult.SwapPayload<Job>();
        }

        Job job = jobQueryResult.Data;
        Result result = job.Assign(workerId);
        if (!result.Ok)
        {
            return result.ToQueryResult<Job>();
        }
        Exception? exception = await jobRepository.UnitOfWork.SaveEntitiesAsync();
        if (exception is not null)
        {
            return QueryResults.ExceptionThrown<Job>(exception);
        }
        return QueryResults.Ok<Job>();
    }

    public async Task<QueryResult<Job>> StartJob(Guid jobId, Guid workerId)
    {
        QueryResult<Job> jobQueryResult = await jobRepository.GetAsync(jobId);
        if (jobQueryResult.NotFound)
        {
            return jobQueryResult.ToQueryResult<Job>();
        }
        
        Job job = jobQueryResult.Data;
        Result result = job.Start(workerId);
        if (!result.Ok)
        {
            return result.ToQueryResult<Job>();
        }
        Exception? exception = await jobRepository.UnitOfWork.SaveEntitiesAsync();
        if (exception is not null)
        {
            return QueryResults.ExceptionThrown<Job>(exception);
        }
        return QueryResults.Ok<Job>();
    }

    public async Task<QueryResult<Job>> CompleteJob(Guid jobId, Guid workerId)
    {
        QueryResult<Job> jobQueryResult = await jobRepository.GetAsync(jobId);
        if (jobQueryResult.NotFound)
        {
            return jobQueryResult.ToQueryResult<Job>();
        }
        
        Job job = jobQueryResult.Data;
        Result result = job.Complete(workerId);
        if (!result.Ok)
        {
            return result.ToQueryResult<Job>();
        }
        
        Exception? exception = await jobRepository.UnitOfWork.SaveEntitiesAsync();
        if (exception is not null)
        {
            return QueryResults.ExceptionThrown<Job>(exception);
        }
        return QueryResults.Ok<Job>();
    }

    public async Task<QueryResult<Job>> FailJob(Guid jobId, Guid workerId)
    {
        QueryResult<Job> jobQueryResult = await jobRepository.GetAsync(jobId);
        if (jobQueryResult.NotFound)
        {
            return jobQueryResult.ToQueryResult<Job>();
        }
        
        Job job = jobQueryResult.Data;
        Result result = job.Fail(workerId);
        if (!result.Ok)
        {
            return result.ToQueryResult<Job>();
        }
        
        Exception? exception = await jobRepository.UnitOfWork.SaveEntitiesAsync();
        if (exception is not null)
        {
            return QueryResults.ExceptionThrown<Job>(exception);
        }
        return QueryResults.Ok<Job>();
    }

}