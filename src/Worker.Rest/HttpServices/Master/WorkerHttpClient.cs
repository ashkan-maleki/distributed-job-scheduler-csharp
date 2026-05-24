using Microsoft.Extensions.Options;
using Worker.Rest.Config;

namespace Worker.Rest.HttpServices.Master;



public interface IWorkerHttpClient
{
    Task<(bool, Domain.Worker?)> Register(string name);
    Task<bool> HeartBeat(Guid workerId);
}

public record RegisterWorkerRequest(string Name);

public class WorkerHttpClient(IOptions<ApiConfig> options,  HttpClient client, ILogger<WorkerHttpClient> logger) : IWorkerHttpClient
{
    public async Task<(bool, Domain.Worker?)> Register(string name)
    {
        HttpResponseMessage httpResponseMessage = await client.PostAsJsonAsync(options.Value.MasterApis.WorkerApis.Registration,
            new RegisterWorkerRequest(name));
        string json = await httpResponseMessage.Content.ReadAsStringAsync();
        if (httpResponseMessage.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            logger.LogError(json);    
            return (false, null);
        }
        
        if (httpResponseMessage.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            logger.LogError(json);    
            return (false, null);
        }
        
        Domain.Worker? worker = await httpResponseMessage.Content.ReadFromJsonAsync<Domain.Worker>();
        return (true, worker);
    }

    public async Task<bool> HeartBeat(Guid workerId)
    {
        HttpResponseMessage httpResponseMessage = await client.GetAsync(options.Value.MasterApis.WorkerApis.HeartBeat + workerId);
        if (httpResponseMessage.StatusCode != System.Net.HttpStatusCode.OK)
        {
            string json = await httpResponseMessage.Content.ReadAsStringAsync();
            logger.LogError(json);
            return false;
        }
        return true;
    }
}