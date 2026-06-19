using System.Net;
using Microsoft.Extensions.Options;
using Shared.Domain.DTOs;
using Shared.Domain.Models;
using Worker.Rest.Config;

namespace Worker.Rest.HttpServices.Master;

public interface IJobHttpClient
{
    public Task<Result<Job>> GetJobAsync(Guid workerId, CancellationToken stoppingToken);
    public Task<Result<Job>> StartJobAsync(Guid workerId, Guid jobId, CancellationToken stoppingToken);
    public Task<Result<Job>> ResultJobAsync(JobCompletionRequest request, CancellationToken stoppingToken);
}

public record JobCompletionRequest(Guid JobId, Guid WorkerId);



public class JobHttpClient(HttpClient client, IOptions<ApiConfig> options) : IJobHttpClient
{
    private MasterJobApis JobApis => options.Value.MasterApis.JobApis;
    public async Task<Result<Job>> GetJobAsync(Guid workerId, CancellationToken stoppingToken)
    {
        string requestUrl = $"{JobApis.Get}{workerId}";
        HttpResponseMessage response = await client.GetAsync(requestUrl, stoppingToken);
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            return new DomainFailure("Master didn't work properly to return a job");
        }
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return new DomainFailure("Master didn't return a job");
        }
        Job? job = await response.Content.ReadFromJsonAsync<Job>(stoppingToken);
        if (job == null)
        {
            return new DomainFailure("Master didn't accept a job");
        }
        return job;
    }

    public async Task<Result<Job>> StartJobAsync(Guid workerId, Guid jobId, CancellationToken stoppingToken)
    {
        string requestUrl = $"{JobApis.Start}{workerId}&jobId={jobId}";
        HttpResponseMessage response = await client.PostAsync(requestUrl, null, stoppingToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return new DomainFailure("Master couldn't find this job in database");
        }
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            return new DomainFailure("Master couldn't start this job for this worker");
        }
        Job? job  = await response.Content.ReadFromJsonAsync<Job>(stoppingToken);
        if (job == null)
        {
            return new DomainFailure("Master didn't accept to start this job");
        }
        return job;
    }

    public async Task<Result<Job>> ResultJobAsync(JobCompletionRequest request, CancellationToken stoppingToken)
    {
        string requestUrl = $"{JobApis.Result}";
        HttpResponseMessage response = await client.PostAsJsonAsync(requestUrl, request, stoppingToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return new DomainFailure("Master couldn't find this job in database");
        }
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return new DomainFailure("Master couldn't accept this result");
        }
        Job? job  = await response.Content.ReadFromJsonAsync<Job>(stoppingToken);
        if (job == null)
        {
            return new DomainFailure("Master didn't accept to finish this job");
        }
        return job;
    }
}