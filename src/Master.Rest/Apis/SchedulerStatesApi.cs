using Master.Domain.Models;
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

    private static async Task<Ok<SchedulerState>> WorkersCountAsync(HttpContext context, SchedulerState schedulerState) => TypedResults.Ok(schedulerState);

    public static async Task<Ok> ScaleAsync(HttpContext context, SchedulerState schedulerState, ScaleWorkersRequest scaleWorkersRequest)
    {
        schedulerState.DesiredNumberOfWorkers = scaleWorkersRequest.Count;
        return TypedResults.Ok();
    }
    
    public record ScaleWorkersRequest(int Count);
}