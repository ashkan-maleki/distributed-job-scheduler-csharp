// See https://aka.ms/new-console-template for more information
using System.Net.Http.Json;
using Shared.Domain.Models;

Console.WriteLine("Welcome to Distributed Job Scheduler Client ClI!");
HttpClient client = new()
{
    BaseAddress = new Uri("http://localhost:5031")
};

Console.WriteLine("Please enter a job name:");
string? jobName = Console.ReadLine();
if (jobName is null)
{
    jobName = "Job 4: Test the app";
}
JobRequest createReq = new(jobName);

HttpResponseMessage createResponse =
    await client.PostAsJsonAsync("/job", createReq);

createResponse.EnsureSuccessStatusCode();

Job? createdJob =
    await createResponse.Content.ReadFromJsonAsync<Job>();

Console.WriteLine(createdJob);

public record JobRequest(string Name);