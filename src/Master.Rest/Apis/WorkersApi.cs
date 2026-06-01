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
        app.MapPost("/worker/scale", ScaleWorkers);
        app.MapPost("/worker/register", RegisterAsync);
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
    private static async Task<HttpResults.Ok> 
        ScaleWorkers(HttpContext context, IWorkerService workerService, ScaleWorkersRequest scaleWorkersRequest)
    {
        IResult result = await workerService.ScaleAsync(scaleWorkersRequest.Count);
        return TypedResults.Ok();
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
        RegisterAsync(HttpContext context, IWorkerService workerService, RegisterWorkerRequest registerWorkerRequest)
    {
        IResult result = await workerService.RegisterAsync(registerWorkerRequest.Name);
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

public record RegisterWorkerRequest(string Name);

public record ScaleWorkersRequest(int Count);

//
// public static class WorkersApiDeprecated
// {
//     public static RouteGroupBuilder MapWorkersApiDeprecated(this RouteGroupBuilder app)
//     {
//         app.MapGet("/worker", AllWorkers);
//         app.MapPost("/worker/scale", ScaleWorkers);
//         app.MapPost("/worker/register", RegisterAsync);
//         app.MapPost("/worker/heartbeat", HeartBeatAsync);
//         return app;
//     }
//
//     private static async Task<Results<Ok, BadRequest<string>>> 
//         ScaleWorkers(HttpContext context, IWorkerService workerService, ScaleWorkersRequest scaleWorkersRequest)
//     {
//         IMessage? error = await workerService.ScaleAsync(scaleWorkersRequest.Count);
//         if (error != null)
//         {
//             return TypedResults.BadRequest(error.ToString());
//         }
//         return TypedResults.Ok();
//     }
//
//     private static async Task<Results<Ok<List<Worker>>, NoContent>> 
//         AllWorkers(HttpContext context, IWorkerService workerService)
//     {
//         List<Worker> workers = await workerService.AllAsync();
//         if (!workers.Any())
//         {
//             return TypedResults.NoContent();
//         }
//         return TypedResults.Ok(workers);
//     }
//     
//     private static async Task<Results<Ok<Worker>, BadRequest<string>, NotFound<string>>> 
//         RegisterAsync(HttpContext context, IWorkerService workerService, RegisterWorkerRequest registerWorkerRequest)
//     {
//         (IMessage? error, Worker? worker) = await workerService.RegisterAsync(registerWorkerRequest.Name);
//         if (error != null && error.Is<WorkerServiceInternalError>())
//         {
//             return TypedResults.BadRequest(error.ToString());
//         }
//         if (error != null && error.Is<WaitingSignalForWorkersError>())
//         {
//             return TypedResults.NotFound(error.ToString());
//         }
//         return TypedResults.Ok(worker);
//     }
//     
//     private static async Task<Results<Ok, BadRequest<string>>> 
//         HeartBeatAsync(HttpContext context, IWorkerService workerService, Guid workerId)
//     {
//         IMessage? error = await workerService.ReportHeartBeatAsync(workerId);
//         if (error != null)
//         {
//             return TypedResults.BadRequest(error.Content);
//         }
//         return TypedResults.Ok();
//     }
// }
