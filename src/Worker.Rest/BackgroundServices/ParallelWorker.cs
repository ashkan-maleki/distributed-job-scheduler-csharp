using Worker.Rest.Works;

namespace Worker.Rest.BackgroundServices;

public class ParallelWorker(ILogger<ParallelWorker> logger) : BackgroundService
{
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        List<Task> workers = [];

        for (int i = 0; i < 5; i++)
        {
            int workerId = i;

            workers.Add(Task.Run(async () =>
                {
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        logger.LogInformation("Worker {id} processing", workerId);
                        await SimpleWork.Run(stoppingToken);
                    }
                }));
        }

        await Task.WhenAll(workers);
    }
}