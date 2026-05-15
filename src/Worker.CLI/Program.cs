// See https://aka.ms/new-console-template for more information

using System.Net.Http.Json;
using Shared.Domain.Models;

Console.WriteLine("Welcome to Distributed Job Scheduler Worker CLI!");

HttpClient client = new()
{
    BaseAddress = new Uri("http://localhost:5031")
};

Guid workerId = Guid.NewGuid();
bool start = true;

while (start)
{

    Console.WriteLine("I'm a worker and I want to get a job to work on it.");
    Console.WriteLine("Getting a job ...");

    Job? assignedJob = await client.GetFromJsonAsync<Job>($"/job?workerId={workerId}");

    Console.WriteLine(assignedJob);

    Guid jobId = assignedJob!.Id;

    Console.WriteLine("I got my job to work on it and I want to start execution.");
    Console.WriteLine("Running job ...");


    HttpResponseMessage startResponse = await client.PostAsync($"/job/start?jobId={jobId}&workerId={workerId}", null);

    startResponse.EnsureSuccessStatusCode();

    Job? runningJob = await startResponse.Content.ReadFromJsonAsync<Job>();

    Console.WriteLine(runningJob);

    Console.WriteLine("I'm a happy worker cause I completed a job and I want to report it to my master.");
    Console.WriteLine("Reporting a completed job ...");

    JobResultRequest resultReq = new(runningJob!.Id, workerId, true, null);

    HttpResponseMessage resultResponse = await client.PostAsJsonAsync("/job/result", resultReq);

    // resultResponse.EnsureSuccessStatusCode();
    string json =  await resultResponse.Content.ReadAsStringAsync();
    var j = json;
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


public record JobResultRequest(Guid JobId, Guid WorkerId, bool Result, string? ErrorMessage);