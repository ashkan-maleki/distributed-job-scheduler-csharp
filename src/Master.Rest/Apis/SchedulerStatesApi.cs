using Master.Domain.Models;
using Master.Domain.Repositories;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Master.Rest.Apis;

public static class SchedulerStatesApi
{
    public static RouteGroupBuilder MapSchedulerStatesApi(this RouteGroupBuilder app)
    {
        app.MapPost("/scheduler-states/scale", ScaleAsync);
        app.MapGet("/scheduler-states/workers-count", WorkersCountAsync);
        return app;
    }

    public record SchedulerStateResponse(int CurrentNumberOfWorkers, int DesiredNumberOfWorkers);

    private static async Task<Ok<SchedulerStateResponse>> WorkersCountAsync(HttpContext context,
        SchedulerState schedulerState, IWorkerRepository workerRepository)
        => TypedResults.Ok(new SchedulerStateResponse(await workerRepository.CountAsync(),
            schedulerState.DesiredNumberOfWorkers));

    public static async Task<Ok> ScaleAsync(HttpContext context, SchedulerState schedulerState,
        ScaleWorkersRequest scaleWorkersRequest)
    {
        schedulerState.DesiredNumberOfWorkers = scaleWorkersRequest.Count;
        return TypedResults.Ok();
    }

    public record ScaleWorkersRequest(int Count);
}