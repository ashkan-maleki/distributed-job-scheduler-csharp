using Master.Domain.Models;
using Master.Domain.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Shared.Domain.Failures;

namespace Master.Rest.Apis;

public static class WorkersApi
{
    public static RouteGroupBuilder MapWorkersApi(this RouteGroupBuilder app)
    {
        app.MapGet("/worker", AllWorkers);
        app.MapPost("/worker/scale", ScaleWorkers);
        app.MapPost("/worker/register", RegisterAsync);
        return app;
    }

    private static async Task<Results<Ok<Worker>, BadRequest<string>, NotFound<string>>> 
        RegisterAsync(HttpContext context, IWorkerService workerService, RegisterWorkerRequest registerWorkerRequest)
    {
        (IError? error, Worker? worker) = await workerService.RegisterAsync(registerWorkerRequest.Name);
        if (error != null && error.Is<WorkerServiceInternalError>())
        {
            return TypedResults.BadRequest(error.ToString());
        }
        if (error != null && error.Is<WaitingSignalForWorkersError>())
        {
            return TypedResults.NotFound(error.ToString());
        }
        return TypedResults.Ok(worker);
    }

    private static async Task<Results<Ok, BadRequest<string>>> 
        ScaleWorkers(HttpContext context, IWorkerService workerService, ScaleWorkersRequest scaleWorkersRequest)
    {
        IError? error = await workerService.ScaleAsync(scaleWorkersRequest.Count);
        if (error != null)
        {
            return TypedResults.BadRequest(error.ToString());
        }
        return TypedResults.Ok();
    }

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
}

public record RegisterWorkerRequest(string Name);

public record ScaleWorkersRequest(int Count);