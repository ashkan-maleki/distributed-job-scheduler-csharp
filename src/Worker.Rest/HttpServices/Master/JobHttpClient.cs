using System.Net;
using Microsoft.Extensions.Options;
using Shared.Domain.DTOs;
using Shared.Domain.Models;
using Worker.Rest.Config;

namespace Worker.Rest.HttpServices.Master;

public interface IJobHttpClient
{
    public Task<(IContentMessage?, Job?)> GetJobAsync(Guid workerId, CancellationToken stoppingToken);
    public Task<(IContentMessage?, Job?)> StartJobAsync(Guid workerId, Guid jobId, CancellationToken stoppingToken);
    public Task<(IContentMessage?, Job?)> ResultJobAsync(JobResultRequest request, CancellationToken stoppingToken);
}

public record JobResultRequest(Guid JobId, Guid WorkerId, bool Successful, string? ErrorMessage);

public class PollingFailureErrorContent(string content) : ErrorContentMessage(content);
public class StartingJobErrorContent(string content) : ErrorContentMessage(content);
public class CompletingJobErrorContent(string content) : ErrorContentMessage(content);

public class JobHttpClient(HttpClient client, IOptions<ApiConfig> options) : IJobHttpClient
{
    private MasterJobApis JobApis => options.Value.MasterApis.JobApis;
    public async Task<(IContentMessage?, Job?)> GetJobAsync(Guid workerId, CancellationToken stoppingToken)
    {
        string requestUrl = $"{JobApis.Get}{workerId}";
        HttpResponseMessage response = await client.GetAsync(requestUrl, stoppingToken);
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            return (new PollingFailureErrorContent("Master didn't work properly to return a job"), null);
        }
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return (new PollingFailureErrorContent("Master didn't return a job"), null);
        }
        Job? job = await response.Content.ReadFromJsonAsync<Job>(stoppingToken);
        return (null, job);
    }

    public async Task<(IContentMessage?, Job?)> StartJobAsync(Guid workerId, Guid jobId, CancellationToken stoppingToken)
    {
        string requestUrl = $"{JobApis.Start}{workerId}&jobId={jobId}";
        HttpResponseMessage response = await client.PostAsync(requestUrl, null, stoppingToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return (new StartingJobErrorContent("Master couldn't find this job in database"), null);
        }
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            return (new StartingJobErrorContent("Master couldn't start this job for this worker"), null);
        }
        Job? job  = await response.Content.ReadFromJsonAsync<Job>(stoppingToken);
        return (null, job);
    }

    public async Task<(IContentMessage?, Job?)> ResultJobAsync(JobResultRequest request, CancellationToken stoppingToken)
    {
        string requestUrl = $"{JobApis.Result}";
        HttpResponseMessage response = await client.PostAsJsonAsync(requestUrl, request, stoppingToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return (new CompletingJobErrorContent("Master couldn't find this job in database"), null);
        }
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return (new CompletingJobErrorContent("Master couldn't accept this result"), null);
        }
        Job? job  = await response.Content.ReadFromJsonAsync<Job>(stoppingToken);
        return (null, job);
    }
}