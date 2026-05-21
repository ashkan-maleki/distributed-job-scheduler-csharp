using Bogus;
using Worker.Rest.HttpServices.Master;

namespace Worker.Rest.BackgroundServices;

public class RegistrationBackgroundService(IWorkerHttpClient httpClient, ILogger<RegistrationBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            Faker  faker = new Faker();
            string name = faker.Company.CompanyName();
            name = name.ToLower().Replace(" ", "-"); 
            (bool registered, Domain.Worker? worker) = await httpClient.Register(name);
            if (registered)
            {
                logger.LogInformation($"Registered {name}");
            }
            else
            {
                logger.LogError($"Failed to register {name}");
                await Task.Delay(10000, stoppingToken);
            }
        }
    }
}