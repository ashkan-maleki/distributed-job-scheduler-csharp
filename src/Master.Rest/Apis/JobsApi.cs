using Master.Domain.Aggregates;
using Master.Domain.Services;
using Master.Domain.Stores;
using Microsoft.AspNetCore.Http.HttpResults;
using Shared.Domain.Failures;

namespace Master.Rest.Apis;

public static class JobsApi
{
    public static RouteGroupBuilder MapJobsApi(this RouteGroupBuilder app)
    {
        app.MapGet("/job", GetJob);
        app.MapPost("/job", CreateJob);
        app.MapPost("/job/start", StartJob);
        app.MapPost("/job/result", SaveResult);
        app.MapGet("/job/all", GetJobList);

        return app;
    }

    private static async Task<Results<Ok<Job>, BadRequest<string>, NotFound<string>>>
        SaveResult(HttpContext context, IJobService jobService, JobResult res)
    {
        IError? err = null;
        Job? job = null;
        
        if (!res.Successful)
        {
            (err, job) = jobService.TryFailJob(res.JobId, res.WorkerId);
        }
        else
        {
            (err, job) = jobService.TryCompleteJob(res.JobId, res.WorkerId);
        }
        
        if (err is not null && err.Is<JobStoreNotFoundError>()) 
        {
            return TypedResults.NotFound(err.ToString());
        }

        if (err is not null && (err.As<Job>() || err.Is<JobStoreOperationError>()))
        {
            return TypedResults.BadRequest(err.ToString());
        }
        return TypedResults.Ok(job);
    }

    private static async Task<Results<Ok<Job>, BadRequest<string>, NotFound<string>>>
        GetJob(HttpContext context, IJobService jobService, Guid workerId)
    {
        (IError? error, Job? job) = jobService.TryAssignJob(workerId);
        if (error is not null && error.Is<JobStoreNotFoundError>())
        {
            return TypedResults.NotFound(error.ToString());
        }

        if (error is not null && (error.As<Job>() || error.Is<JobStoreOperationError>()))
        {
            return TypedResults.BadRequest(error.ToString());
        }

        return TypedResults.Ok(job);
    }

    private static async Task<Results<Ok<Job>, BadRequest<string>>>
        CreateJob(HttpContext context, JobRequest req, IJobService jobService)
    {
        (IError? error, Job? job) = jobService.TryQueueJob(req.Name);
        if (error is not null)
        {
            return TypedResults.BadRequest(error.ToString());
        }

        return TypedResults.Ok(job);
    }
    
    private static async Task<Results<Ok<Job>, BadRequest<string>, NotFound<string>>>
        StartJob(HttpContext context, IJobService jobService, Guid jobId, Guid workerId)
    {
        (IError? error, Job? job) = jobService.TryStartJob(jobId, workerId);
        if (error is not null && error.Is<JobStoreNotFoundError>()) 
        {
            return TypedResults.NotFound(error.ToString());
        }

        if (error is not null && (error.As<Job>() || error.Is<JobStoreOperationError>()))
        {
            return TypedResults.BadRequest(error.ToString());
        }
        return TypedResults.Ok(job);
    }
    
    private static async Task<Results<Ok<List<Job>>, NoContent>> GetJobList(HttpContext context, IJobService jobService)
    {
        List<Job> jobs = jobService.Jobs;
        if (!jobs.Any())
        {
            return TypedResults.NoContent();
        }

        return TypedResults.Ok(jobs);
    }
}