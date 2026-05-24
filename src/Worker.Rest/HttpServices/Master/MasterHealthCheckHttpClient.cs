using Microsoft.Extensions.Options;
using Worker.Rest.Config;

namespace Worker.Rest.HttpServices.Master;

public interface IMasterHealthCheckHttpClient
{
    Task<bool> MasterAvailableAsync(CancellationToken stoppingToken);
}

public class MasterHealthCheckHttpClient(HttpClient client, IOptions<ApiConfig> options) : IMasterHealthCheckHttpClient
{
    public async Task<bool> MasterAvailableAsync(CancellationToken stoppingToken)
    {
        if (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(options.Value.MasterApis?.HealthCheck, stoppingToken);
                if (response.IsSuccessStatusCode)
                {
                    return true;   
                }  
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Master unavailable: {ex.Message}");
                return false;
            }
        }
        return false;
    }
}