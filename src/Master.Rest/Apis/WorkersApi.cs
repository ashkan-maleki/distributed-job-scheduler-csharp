using Master.Domain.Models;
using Master.Domain.Services;
using Master.Rest.DTOs;
using Microsoft.AspNetCore.Http.HttpResults;
using Shared.Domain.DTOs;

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

    private static async Task<Results<Ok, BadRequest<string>>> 
        ScaleWorkers(HttpContext context, IWorkerService workerService, ScaleWorkersRequest scaleWorkersRequest) =>
        HttpTypedResult.From<Ok, BadRequest<string>>(await workerService.ScaleAsync(scaleWorkersRequest.Count));

    private static async Task<Results<Ok<List<Worker>>, NoContent>> 
        AllWorkers(HttpContext context, IWorkerService workerService)
    {
        List<Worker> workers = await workerService.AllAsync();
        if (!workers.Any())
        {
            return TypedResults.NoContent();
        }
        return TypedResults.Ok(workers);
    }
    
    private static async Task<Results<Ok<Worker>, BadRequest<string>, NotFound<string>>> 
        RegisterAsync(HttpContext context, IWorkerService workerService, RegisterWorkerRequest registerWorkerRequest) =>
        HttpTypedResult.From<Worker, Ok<Worker>, BadRequest<string>, NotFound<string>>(await workerService.RegisterAsync(registerWorkerRequest.Name));

    private static async Task<Results<Ok, BadRequest<string>>> 
        HeartBeatAsync(HttpContext context, IWorkerService workerService, Guid workerId) =>
        HttpTypedResult.From<Ok, BadRequest<string>>(await workerService.ReportHeartBeatAsync(workerId));
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
