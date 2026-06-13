using Microsoft.Extensions.Options;
using Shared.Domain.DTOs;
using Worker.Rest.Config;

namespace Worker.Rest.HttpServices.Master;



public interface IWorkerHttpClient
{
    Task<Result<Domain.Worker>> Register();
    Task<bool> HeartBeat(Guid workerId);
}



public class WorkerHttpClient(IOptions<ApiConfig> options,  HttpClient client, ILogger<WorkerHttpClient> logger) : IWorkerHttpClient
{
    public async Task<Result<Domain.Worker>> Register()
    {
        HttpResponseMessage httpResponseMessage = await client.GetAsync(options.Value.MasterApis.WorkerApis.Registration);
        
        if (httpResponseMessage.StatusCode is (System.Net.HttpStatusCode.BadRequest 
            or System.Net.HttpStatusCode.NotFound
            or System.Net.HttpStatusCode.InternalServerError))
        {
            string json = await httpResponseMessage.Content.ReadAsStringAsync();
            logger.LogError(json);    
            return new DomainFailure(json);
        }
        
        Domain.Worker worker = (await httpResponseMessage.Content.ReadFromJsonAsync<Domain.Worker>())!;
        return worker;
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