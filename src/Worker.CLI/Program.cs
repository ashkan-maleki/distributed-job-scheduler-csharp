// See https://aka.ms/new-console-template for more information

using System.Net.Http.Json;
using DistributedJobScheduler.Shared;

Console.WriteLine("Welcome to Distributed Job Scheduler Worker CLI!");

HttpClient client = new()
{
    BaseAddress = new Uri("http://localhost:5031")
};

bool start = true;

while (start)
{

    Console.WriteLine("I'm a worker and I want to get a job to work on it.");
    Console.WriteLine("Getting a job ...");

    Job? assignedJob = await client.GetFromJsonAsync<Job>("/job");

    Console.WriteLine(assignedJob);

    Guid jobId = assignedJob!.Id;

    Console.WriteLine("I got my job to work on it and I want to start execution.");
    Console.WriteLine("Running job ...");


    HttpResponseMessage startResponse = await client.PostAsync($"/start?jobId={jobId}", null);

    startResponse.EnsureSuccessStatusCode();

    Job? runningJob = await startResponse.Content.ReadFromJsonAsync<Job>();

    Console.WriteLine(runningJob);

    Console.WriteLine("I'm a happy worker cause I completed a job and I want to report it to my master.");
    Console.WriteLine("Reporting a completed job ...");

    JobResultRequest resultReq = new(runningJob!.Id, true, null);

    HttpResponseMessage resultResponse = await client.PostAsJsonAsync("/result", resultReq);

    resultResponse.EnsureSuccessStatusCode();

    Job? completedJob = await resultResponse.Content.ReadFromJsonAsync<Job>();

    Console.WriteLine(completedJob);
    Console.WriteLine("I'm a happy because master sent me a kiss for completing my job successfully.");
    Console.WriteLine("Do you want me to another job for you, my master? (Yes/No)");
    string? answer = Console.ReadLine();
    if (answer == "Yes")
    {
        start = true;
        Console.WriteLine("I'm at your service my master.");
    }
    else
    {
        start = false;
        Console.WriteLine("Have a nice day, my master!");
    }
}


public record JobResultRequest(Guid JobId, bool Result, string? ErrorMessage);

public enum Order
{
    Assign=1,
    Start,
    Complete,
}