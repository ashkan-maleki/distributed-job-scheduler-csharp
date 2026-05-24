using Microsoft.Extensions.Options;
using Worker.Rest.Config;

namespace Worker.Rest.HttpServices.Master;

public static class HttpClientServiceRegistration
{
    public static IServiceCollection AddMasterHttpClients(this IServiceCollection services)
    {
        void ConfigureClient(IServiceProvider serviceProvider, HttpClient client)
        {
            IOptions<ApiConfig>  config = serviceProvider.GetRequiredService<IOptions<ApiConfig>>();
            client.BaseAddress = new Uri(config.Value.MasterApis.BaseAddress);
        }
        
        services.AddHttpClient<IMasterHealthCheckHttpClient, MasterHealthCheckHttpClient>(ConfigureClient);
        services.AddHttpClient<IWorkerHttpClient, WorkerHttpClient>(ConfigureClient);
        services.AddHttpClient<IJobHttpClient, JobHttpClient>(ConfigureClient);
        return services;
    }
}