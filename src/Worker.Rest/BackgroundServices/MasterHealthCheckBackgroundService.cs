using Worker.Rest.Contexts;
using Worker.Rest.HttpServices.Master;

namespace Worker.Rest.BackgroundServices;

public class MasterHealthCheckBackgroundService(ILogger<MasterHealthCheckBackgroundService> logger,
    IMasterHealthCheckHttpClient httpClient, WorkerContext context) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            bool available = await httpClient.MasterAvailableAsync(stoppingToken);
            if (available)
            {
                context.MasterHeartbeatTime =  DateTime.Now;
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
            else
            {
                logger.LogError("Master unavailable");
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }
    }
}