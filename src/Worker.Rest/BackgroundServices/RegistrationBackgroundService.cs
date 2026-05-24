using Bogus;
using Microsoft.EntityFrameworkCore;
using Worker.Rest.Contexts;
using Worker.Rest.EF;
using Worker.Rest.HttpServices.Master;

namespace Worker.Rest.BackgroundServices;

public class RegistrationBackgroundService(
    ILogger<RegistrationBackgroundService> logger,
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

            Faker faker = new Faker();
            string name = faker.Company.CompanyName();
            name = name.ToLower().Replace(" ", "-");
            (bool registered, Domain.Worker? worker) = await httpClient.Register(name);
            if (registered && worker is not null)
            {
                await using WorkerDbContext db = await factory.CreateDbContextAsync(stoppingToken);
                worker.Register();
                await db.Workers.AddAsync(worker, stoppingToken);
                await db.SaveChangesAsync(stoppingToken);
                logger.LogInformation($"Registered {name} (worker id: {worker.Id})");
            }
            else
            {
                logger.LogError($"Failed to register {name}");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}