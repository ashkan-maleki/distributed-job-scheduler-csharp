using System.Net.Http.Json;
using Bogus;
using Shared.Domain.Models;

namespace Control.CLI;

public class JobService(HttpClient client)
{
    
    public async Task New()
    {
        // Console.WriteLine("Please enter a job name:");
        // string? jobName = Console.ReadLine();
        Faker faker = new();

        string jobName = faker.Commerce.Product();
        if (jobName is null)
        {
            jobName = "Job 4: Test the app";
        }
        JobRequest createReq = new(jobName);

        HttpResponseMessage createResponse =
            await client.PostAsJsonAsync("/api/job", createReq);

        createResponse.EnsureSuccessStatusCode();

        Job? createdJob =
            await createResponse.Content.ReadFromJsonAsync<Job>();

        Console.WriteLine(createdJob);

    }
    
    public async Task All()
    {
        List<Job>? jobs = await client.GetFromJsonAsync<List<Job>>("/api/job/all");

        if (jobs is null)
        {
            Console.WriteLine("No jobs found");
        }
        
        foreach (Job job in jobs)
        {
            Console.WriteLine(job);
        }
    }
}