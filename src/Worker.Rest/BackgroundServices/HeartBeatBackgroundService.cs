using Microsoft.EntityFrameworkCore;
using Worker.Rest.Contexts;
using Worker.Rest.EF;
using Worker.Rest.HttpServices.Master;

namespace Worker.Rest.BackgroundServices;

public class HeartBeatBackgroundService(
    ILogger<HeartBeatBackgroundService> logger,
    IWorkerHttpClient httpClient,
    IDbContextFactory<WorkerDbContext> factory,
    WorkerContext context) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (context.MasterUnavailable)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                continue;
            }

            await using WorkerDbContext dbContext = await factory.CreateDbContextAsync(stoppingToken);
            Domain.Worker? worker = await dbContext.Workers
                .Where(w => w.HeartBeatReportedAt - DateTime.UtcNow > TimeSpan.FromSeconds(4))
                .FirstOrDefaultAsync(stoppingToken);

            if (worker == null)
            {
                logger.LogInformation("Worker not found");
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                continue;
            }

            if (await httpClient.HeartBeat(worker.Id))
            {
                worker.ReportHeartBeat();
                await dbContext.SaveChangesAsync(stoppingToken);
            }
        }
    }
}