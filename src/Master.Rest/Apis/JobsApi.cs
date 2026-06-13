using Master.Domain.Aggregates;
using Master.Domain.Services;
using HttpResults = Microsoft.AspNetCore.Http.HttpResults;
using Shared.Domain.DTOs;
using IResult = Shared.Domain.DTOs.IResult;

namespace Master.Rest.Apis;

public record JobRequest(string Name);
public record JobCompletionResult(Guid JobId, Guid WorkerId);
public record JobFailureResult(Guid JobId, Guid WorkerId, string ErrorMessage);

public static class JobsApi
{
    public static RouteGroupBuilder MapJobsApi(this RouteGroupBuilder app)
    {
        app.MapGet("/job/all", GetJobList);
        app.MapGet("/job", GetJob);
        app.MapPost("/job", CreateJob);
        app.MapPost("/job/start", StartJob);
        app.MapPost("/job/complete", CompleteJob);
        app.MapPost("/job/fail", FailJob);

        return app;
    }

    private static HttpResults.Results<HttpResults.Ok<Job>, HttpResults.NotFound<string>,
            HttpResults.BadRequest<string>, HttpResults.InternalServerError<string>>
        MapResultsToHttpTypedResults(Result<Job> result)
    {
        switch (result.WrappedResult)
        {
            case CriticalError criticalError:
                return TypedResults.InternalServerError(criticalError.Message);
            case DomainFailure domainFailure:
                return TypedResults.BadRequest(domainFailure.Message);
            case NotFound notFound:
                return TypedResults.NotFound(notFound.Message);
            case Object<Job> objectJob:
                return TypedResults.Ok(objectJob.Value);
            default:
                return TypedResults.InternalServerError("unknown error");
        }
    }
    
    private static async Task<HttpResults.Results<HttpResults.Ok<Job>, HttpResults.NotFound<string>, HttpResults.BadRequest<string>, HttpResults.InternalServerError<string>>>
        CompleteJob(HttpContext context, IJobService jobService, JobCompletionResult res)
    {
        Result<Job> result = await jobService.CompleteJobAsync(res.JobId, res.WorkerId);
        return MapResultsToHttpTypedResults(result);
    }
    
    private static async Task<HttpResults.Results<HttpResults.Ok<Job>, HttpResults.NotFound<string>, HttpResults.BadRequest<string>, HttpResults.InternalServerError<string>>>
        FailJob(HttpContext context, IJobService jobService, JobFailureResult res)
    {
        
        Result<Job> result = await jobService.FailJobAsync(res.JobId, res.WorkerId);
        return MapResultsToHttpTypedResults(result);
    }

    private static async Task<HttpResults.Results<HttpResults.Ok<Job>, HttpResults.NotFound<string>, HttpResults.BadRequest<string>, HttpResults.InternalServerError<string>>>
        GetJob(HttpContext context, IJobService jobService, Guid workerId)
    {
        Result<Job> result = await jobService.AssignJobAsync(workerId);
        return MapResultsToHttpTypedResults(result);
    }

    private static async Task<HttpResults.Results<HttpResults.Ok<Job>, HttpResults.NotFound<string>, HttpResults.BadRequest<string>, HttpResults.InternalServerError<string>>>
        CreateJob(HttpContext context, JobRequest req, IJobService jobService)
    {
        Result<Job> result = await jobService.QueueJobAsync(req.Name);
        return MapResultsToHttpTypedResults(result);
    }

    private static async Task<HttpResults.Results<HttpResults.Ok<Job>, HttpResults.NotFound<string>, HttpResults.BadRequest<string>, HttpResults.InternalServerError<string>>>
        StartJob(HttpContext context, IJobService jobService, Guid jobId, Guid workerId)
    {
        Result<Job> result = await jobService.StartJobAsync(jobId, workerId);
        return MapResultsToHttpTypedResults(result);
    }

    private static async Task<HttpResults.Results<HttpResults.Ok<List<Job>>, HttpResults.NoContent>> GetJobList(HttpContext context, IJobService jobService)
    {
        List<Job> jobs = await jobService.AllAsync();
        if (!jobs.Any())
        {
            return TypedResults.NoContent();
        }

        return TypedResults.Ok(jobs);
    }
}