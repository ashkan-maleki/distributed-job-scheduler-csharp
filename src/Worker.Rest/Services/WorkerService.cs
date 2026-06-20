using Shared.Domain.DTOs;
using Shared.Domain.Models;
using Worker.Rest.HttpServices.Master;
using Worker.Rest.Stores;
using Worker.Rest.Works;
using IResult = Shared.Domain.DTOs.IResult;

namespace Worker.Rest.Services;


public class WorkerService(IWorkerStore store, IWorkerHttpClient httpClient,
    ILogger<WorkerService> logger) : IWorkerService
{
    public async Task<Result<Domain.Worker>> RegisterAsync(CancellationToken stoppingToken)
    {
        Result<Domain.Worker> result = await httpClient.Register();
        if (result.DomainFailed)
        {
            return new DomainFailure($"Failed to register: " + result.DomainFailureResult.Message);
        }
        
        Domain.Worker worker = result.OkResult.Value;
        
        worker.Register();
        await store.AddAsync(worker, stoppingToken);
        IResult saveResult = await store.UnitOfWork.SaveEntitiesAsync(stoppingToken);
        if (saveResult is CriticalError error)
        {
            return error;
        }

        logger.LogInformation($"Registered the worker with id: {worker.Id}, named: {worker.Name}");
        return worker;
    }

    public async Task HeartBeatAsync(Guid workerId, CancellationToken stoppingToken)
    {
        Result<Domain.Worker> result = await httpClient.Register();
        if (result.DomainFailed)
        {
            logger.LogError($"Failed to register: " + result.DomainFailureResult.Message);
            return;
        }
        
        Domain.Worker worker = result.Value;
        if (worker.ShouldNotReportHeartBeat)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            return;
        }

        if (await httpClient.HeartBeat(worker.Id))
        {
            worker.ReportHeartBeat();
            logger.LogInformation("Heartbeat complete at {time}", DateTimeOffset.Now);
            await store.UnitOfWork.SaveChangesAsync(stoppingToken);
        }
    }
}

public interface IWorkerService
{
    Task<Result<Domain.Worker>> RegisterAsync(CancellationToken stoppingToken);
    Task HeartBeatAsync(Guid workerId, CancellationToken stoppingToken);

}