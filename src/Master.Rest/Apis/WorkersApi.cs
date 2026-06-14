using Master.Domain.Models;
using Master.Domain.Services;
using Shared.Domain.DTOs;
using HttpResults = Microsoft.AspNetCore.Http.HttpResults;
using IResult = Shared.Domain.DTOs.IResult;

namespace Master.Rest.Apis;

public static class WorkersApi
{
    public static RouteGroupBuilder MapWorkersApi(this RouteGroupBuilder app)
    {
        app.MapGet("/worker", AllWorkers);
        app.MapPost("/worker/scale", Suicide);
        app.MapGet("/worker/register", RegisterAsync);
        app.MapPost("/worker/heartbeat", HeartBeatAsync);
        return app;
    }

    private static async Task<HttpResults.Results<HttpResults.Ok<List<Worker>>, HttpResults.NoContent>>
        AllWorkers(HttpContext context, IWorkerService workerService)
    {
        List<Worker> workers = await workerService.AllAsync();
        if (workers.Any())
        {
            return TypedResults.NoContent();
        }

        return TypedResults.Ok(workers);
    }

    // private static async Task<Results<HttpResults.Ok, HttpResults.NotFound, BadRequest<string>, InternalServerError<string>>> 
    private static async Task<HttpResults.Ok<SuicideResponse>>
        Suicide(HttpContext context, IWorkerService workerService, SuicideRequest request)
    {
        SuicideResponse response = new(await workerService.CommitSuicideAsync(request.WorkerId));
        return TypedResults.Ok(response);
    }

    private static HttpResults.Results<HttpResults.Ok<Worker>, HttpResults.NotFound<string>,
            HttpResults.BadRequest<string>, HttpResults.InternalServerError<string>>
        MapResultsToHttpTypedResults(IResult result)
    {
        switch (result)
        {
            case CriticalError criticalError:
                return TypedResults.InternalServerError(criticalError.Message);
            case DomainFailure domainFailure:
                return TypedResults.BadRequest(domainFailure.Message);
            case NotFound notFound:
                return TypedResults.NotFound(notFound.Message);
            case Object<Worker> objectJob:
                return TypedResults.Ok(objectJob.Value);
            default:
                return TypedResults.InternalServerError("unknown error");
        }
    }

    private static async Task<HttpResults.Results<HttpResults.Ok<Worker>, HttpResults.NotFound<string>,
            HttpResults.BadRequest<string>, HttpResults.InternalServerError<string>>>
        RegisterAsync(HttpContext context, IConcurrentRegistrationService registrationService)
    {
        IResult result = await registrationService.RegisterAsync();
        return MapResultsToHttpTypedResults(result);
    }

    private static async Task<HttpResults.Results<HttpResults.Ok<Worker>, HttpResults.NotFound<string>,
            HttpResults.BadRequest<string>, HttpResults.InternalServerError<string>>>
        HeartBeatAsync(HttpContext context, IWorkerService workerService, Guid workerId)
    {
        IResult result = await workerService.ReportHeartBeatAsync(workerId);
        return MapResultsToHttpTypedResults(result);
    }
}

public record SuicideRequest(Guid WorkerId);

public record SuicideResponse(bool ShouldICommitSuicide);