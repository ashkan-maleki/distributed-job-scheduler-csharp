using Worker.Rest.HttpServices.Master;

namespace Worker.Rest.BackgroundServices;

public class MasterHealthCheckBackgroundService(IMasterHealthCheckHttpClient httpClient) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            bool available =
                await httpClient.MasterAvailableAsync(stoppingToken);

            if (!available)
            {
                Console.WriteLine("Master unavailable");
            }

            await Task.Delay(10000, stoppingToken);
        }
    }
}