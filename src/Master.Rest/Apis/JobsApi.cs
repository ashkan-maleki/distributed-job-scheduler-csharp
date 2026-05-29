using Master.Domain.Aggregates;
using Master.Domain.Services;
using Master.Rest.DTOs;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Master.Rest.Apis;

public static class JobsApi
{
    public static RouteGroupBuilder MapJobsApi(this RouteGroupBuilder app)
    {
        app.MapGet("/job/all", GetJobList);
        app.MapGet("/job", GetJob);
        app.MapPost("/job", CreateJob);
        app.MapPost("/job/start", StartJob);
        app.MapPost("/job/result", SaveResult);

        return app;
    }

    private static async Task<Results<Ok<Job>, NotFound<string>, BadRequest<string>>>
        SaveResult(HttpContext context, IJobService jobService, JobResult res)
    {
        if (!res.Successful)
        {
            return HttpTypedResult.From<Job, Ok<Job>, NotFound<string>, BadRequest<string>>(
                await jobService.FailJob(res.JobId, res.WorkerId));
        }

        return HttpTypedResult.From<Job, Ok<Job>, NotFound<string>, BadRequest<string>>(
            await jobService.CompleteJob(res.JobId, res.WorkerId));
    }

    private static async Task<Results<Ok<Job>, BadRequest<string>, NotFound<string>>>
        GetJob(HttpContext context, IJobService jobService, Guid workerId) =>
        HttpTypedResult.From<Job, Ok<Job>, BadRequest<string>, NotFound<string>>(
            await jobService.AssignJob(workerId));

    private static async Task<Results<Ok<Job>, BadRequest<string>>>
        CreateJob(HttpContext context, JobRequest req, IJobService jobService) =>
        HttpTypedResult.From<Job, Ok<Job>, BadRequest<string>>(await jobService.QueueJob(req.Name));

    private static async Task<Results<Ok<Job>, BadRequest<string>, NotFound<string>>>
        StartJob(HttpContext context, IJobService jobService, Guid jobId, Guid workerId) =>
        HttpTypedResult.From<Job, Ok<Job>, BadRequest<string>, NotFound<string>>(
            await jobService.StartJob(jobId, workerId));

    private static async Task<Results<Ok<List<Job>>, NoContent>> GetJobList(HttpContext context, IJobService jobService)
    {
        List<Job> jobs = await jobService.AllAsync();
        if (!jobs.Any())
        {
            return TypedResults.NoContent();
        }

        return TypedResults.Ok(jobs);
    }
}

public static class JobsApiDeprecated
{
    // private static async Task<Results<Ok<Job>, BadRequest<string>, NotFound<string>>>
    //     StartJob(HttpContext context, IJobService jobService, Guid jobId, Guid workerId)
    // {
    //     (IMessage? error, Job? job) = await jobService.StartJob(jobId, workerId);
    //     if (error is not null && error.Is<JobRepositoryNotFoundError>()) 
    //     {
    //         return TypedResults.NotFound(error.ToString());
    //     }
    //
    //     if (error is not null && (error.PayloadOfType<Job>() || error.Is<JobRepositoryOperationError>()))
    //     {
    //         return TypedResults.BadRequest(error.ToString());
    //     }
    //     return TypedResults.Ok(job);
    // }
    //
    // private static async Task<Results<Ok<Job>, BadRequest<string>>>
    //     CreateJob(HttpContext context, JobRequest req, IJobService jobService)
    // {
    //     (IMessage? error, Job? job) = await jobService.QueueJob(req.Name);
    //     if (error is not null)
    //     {
    //         return TypedResults.BadRequest(error.ToString());
    //     }
    //
    //     return TypedResults.Ok(job);
    // }
    //
    // private static async Task<Results<Ok<Job>, BadRequest<string>, NotFound<string>>>
    //     GetJob(HttpContext context, IJobService jobService, Guid workerId)
    // {
    //     (IMessage? error, Job? job) = await jobService.AssignJob(workerId);
    //     if (error is not null && error.Is<JobRepositoryNotFoundError>())
    //     {
    //         return TypedResults.NotFound(error.ToString());
    //     }
    //
    //     if (error is not null && (error.PayloadOfType<Job>() || error.Is<JobRepositoryOperationError>()))
    //     {
    //         return TypedResults.BadRequest(error.ToString());
    //     }
    //
    //     return TypedResults.Ok(job);
    // }
}