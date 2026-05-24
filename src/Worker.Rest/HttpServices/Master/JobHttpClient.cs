using System.Net;
using Microsoft.Extensions.Options;
using Shared.Domain.Messages;
using Shared.Domain.Models;
using Worker.Rest.Config;

namespace Worker.Rest.HttpServices.Master;

public interface IJobHttpClient
{
    public Task<(IError?, Job?)> GetJobAsync(Guid workerId, CancellationToken stoppingToken);
}

public class PollingFailureError(string message) : Error<IJobHttpClient>(message);

public class JobHttpClient(HttpClient client, IOptions<ApiConfig> options) : IJobHttpClient
{
    public async Task<(IError?, Job?)> GetJobAsync(Guid workerId, CancellationToken stoppingToken)
    {
        string requestUrl = $"{options.Value.MasterApis?.JobApis?.Get}{workerId}";
        HttpResponseMessage response = await client.GetAsync(requestUrl, stoppingToken);
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            return (new PollingFailureError("Master didn't work properly to return a job"), null);
        }
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            return (new PollingFailureError("Master didn't return a job"), null);
        }
        Job? job = await response.Content.ReadFromJsonAsync<Job>(stoppingToken);
        return (null, job);
    }
}