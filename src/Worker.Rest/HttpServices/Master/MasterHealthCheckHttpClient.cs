using Worker.Rest.Config;

namespace Worker.Rest.HttpServices.Master;

public interface IMasterHealthCheckHttpClient
{
    Task<bool> MasterAvailableAsync(CancellationToken stoppingToken);
}

public class MasterHealthCheckHttpClient(AppConfig appConfig,  HttpClient client) : IMasterHealthCheckHttpClient
{
    
    public async Task<bool> MasterAvailableAsync(CancellationToken stoppingToken)
    {
        try
        {
            HttpResponseMessage response = await client.GetAsync(appConfig.MasterHealthCheck, stoppingToken);

            response.EnsureSuccessStatusCode();

            Console.WriteLine("Master available");

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Master unavailable: {ex.Message}");
            return false;
        }
    }
}