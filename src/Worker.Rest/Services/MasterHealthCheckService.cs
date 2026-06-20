using Worker.Rest.HttpServices.Master;

namespace Worker.Rest.Services;

public class MasterHealthCheckService(ILogger<MasterHealthCheckService> logger,
    IMasterHealthCheckHttpClient httpClient) : IMasterHealthCheckService
{
    public async Task IsMasterAvailableAsync(CancellationToken stoppingToken)
    {
        bool available = await httpClient.MasterAvailableAsync(stoppingToken);
        while (!available)
        {
            logger.LogError("Master unavailable");
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            available = await httpClient.MasterAvailableAsync(stoppingToken);
        }
        logger.LogInformation($"Master responded at {DateTime.Now}");
    }
}

public interface IMasterHealthCheckService
{
    public Task IsMasterAvailableAsync(CancellationToken stoppingToken);
}