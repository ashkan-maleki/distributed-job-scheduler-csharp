using Worker.Rest.Config;

namespace Worker.Rest.HttpServices.Master;



public interface IWorkerHttpClient
{
    Task<(bool, Domain.Worker?)> Register(string name);
}

public record RegisterWorkerRequest(string Name);

public class WorkerHttpClient(AppConfig appConfig,  HttpClient client, ILogger<WorkerHttpClient> logger) : IWorkerHttpClient
{
    public async Task<(bool, Domain.Worker?)> Register(string name)
    {
        HttpResponseMessage httpResponseMessage = 
            await client.PostAsJsonAsync(appConfig.RegistrationApi, new RegisterWorkerRequest(name));
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
}