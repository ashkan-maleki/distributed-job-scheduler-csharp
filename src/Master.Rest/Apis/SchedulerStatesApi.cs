using Master.Domain.Models;
using Master.Domain.Repositories;
using Master.Domain.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Shared.Domain.DTOs;
using IResult = Shared.Domain.DTOs.IResult;
using Ok = Microsoft.AspNetCore.Http.HttpResults.Ok;

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

    private static async Task<Microsoft.AspNetCore.Http.HttpResults.Ok<SchedulerStateResponse>> WorkersCountAsync(HttpContext context,
         IDesiredStateService desiredStateService, IWorkerRepository workerRepository) =>
        TypedResults.Ok(new SchedulerStateResponse(await workerRepository.CountAsync(),
            await desiredStateService.DesiredNumberOfWorkersAsync()));

    public static async Task<Results<Ok, InternalServerError<string>>> ScaleAsync(HttpContext context, IDesiredStateService  desiredStateService, 
        ScaleWorkersRequest scaleWorkersRequest)
    {
        IResult result = await desiredStateService.ScaleAsync(scaleWorkersRequest.DesiredNumberOfWorkers);
        if (result is CriticalError criticalError)
        {
            return TypedResults.InternalServerError(criticalError.Message);
        }
        return TypedResults.Ok();
    }

    public record ScaleWorkersRequest(int DesiredNumberOfWorkers);
}