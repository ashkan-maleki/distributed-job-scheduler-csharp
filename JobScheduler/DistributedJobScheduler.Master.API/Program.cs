using System.Collections.Concurrent;
using DistributedJobScheduler.Shared;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

object @lock = new object();

Job item = new Job("Job 1: Wash dishes");
Job item1 = new Job("Job 2: Clean your room");
Job job1 = new Job("Job 3: Work on the garden");

ConcurrentDictionary<Guid, Job> jobs = new();

jobs.TryAdd(job1.Id, job1);
jobs.TryAdd(item.Id, item);
jobs.TryAdd(item1.Id, item1);

builder.Services.AddSingleton(jobs);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost("/job", (JobRequest req, ConcurrentDictionary<Guid, Job> jobs) =>
    {
        Job job = new Job(req.Name);
        if (!jobs.TryAdd(job.Id, job))
        {
            return Results.BadRequest($"Job {req.Name} already exists.");
        }
        return Results.Ok(job);
    })
    .WithName("SaveJob");
app.MapGet("/job", (ConcurrentDictionary<Guid, Job> jobs) =>
    {
        lock (@lock)
        {
            Job? queuedJob = jobs.Values.FirstOrDefault(j => j.State == JobState.Queued);
            if (queuedJob == null) 
            {
                return Results.NoContent();
            }
            Job assignedJob = queuedJob with { State = JobState.Assigned };
            // if (!jobsDic.TryUpdate(assignedJob.Id, assignedJob, queuedJob))
            // {
            //     return Results.BadRequest($"This job, {assignedJob.Name}, is already assigned.");
            // }
            jobs[assignedJob.Id] = assignedJob;
            return Results.Ok(assignedJob);
        }
    })
    .WithName("GetJob");
app.MapPost("/result", (JobResult res, ConcurrentDictionary<Guid, Job> jobs) =>
    {
        if (!jobs.TryGetValue(res.JobId, out Job job))
        {
            return Results.BadRequest("You're so mean for trying to hack us");            
        }
        if (job.State == JobState.Completed)
        {
            return Results.BadRequest("Job is already completed.");
        }
        if (!res.Result)
        {
            return Results.BadRequest("Thank you for your loyal service, but you failed.");
        }
        Job completedJob = job with { State = JobState.Completed };
        if (!jobs.TryUpdate(res.JobId, completedJob, job))
        {
            return Results.BadRequest($"Job {res.JobId} already changed.");
        }
        return Results.Ok(completedJob);
    })
    .WithName("SaveResult");

app.Run();


public record JobRequest(string Name);

public record JobResult(Guid JobId, bool Result, string ErrorMessage);
