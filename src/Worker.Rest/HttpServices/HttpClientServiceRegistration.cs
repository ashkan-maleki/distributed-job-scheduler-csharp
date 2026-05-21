using Worker.Rest.Config;
using Worker.Rest.HttpServices.Master;

namespace Worker.Rest.HttpServices;

public static class HttpClientServiceRegistration
{
    public static IServiceCollection AddMasterHttpClients(this IServiceCollection services, AppConfig appConfig)
    {
        Uri baseAddress = new Uri(appConfig.MasterIpAddress);
        services.AddHttpClient<IMasterHealthCheckHttpClient, MasterHealthCheckHttpClient>(client =>
        {
            client.BaseAddress = baseAddress;
        });
        services.AddHttpClient<IWorkerHttpClient, WorkerHttpClient>(client =>
        {
            client.BaseAddress = baseAddress;
        });
        return services;
    }
}