using Master.Domain.Models;
using Master.Domain.Repositories;

namespace Master.Rest.BackgroundServices;

public class WorkersCountBackgroundService(
    ILogger<WorkersCountBackgroundService> logger,
    DesiredState desiredState,
    IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        IWorkerRepository workerRepository = scope.ServiceProvider.GetRequiredService<IWorkerRepository>();
        int count = await workerRepository.CountAsync();
        logger.LogInformation($"WorkersCount {count}");
        desiredState.DesiredNumberOfWorkers = count;
    }
}