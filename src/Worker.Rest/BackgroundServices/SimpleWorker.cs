using Worker.Rest.Works;

namespace Worker.Rest.BackgroundServices;

public class SimpleWorker(ILogger<SimpleWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await SimpleWork.Run(stoppingToken);
        }

        logger.LogInformation("Worker stopped.");
    }
}