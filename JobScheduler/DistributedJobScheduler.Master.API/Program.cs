using System.Collections.Concurrent;
using DistributedJobScheduler.Shared;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

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
    app.UseSwagger();
    app.UseSwaggerUI();
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
            if (!jobs.TryUpdate(assignedJob.Id, assignedJob, queuedJob))
            {
                return Results.BadRequest($"This job, {assignedJob.Name}, is already assigned.");
            }
            return Results.Ok(assignedJob);
        }
    })
    .WithName("GetJob");
app.MapPost("/start", (Guid jobId, ConcurrentDictionary<Guid, Job> jobs) =>
    {
        if (!jobs.TryGetValue(jobId, out Job job))
        {
            return Results.BadRequest("You're so mean for trying to hack us");            
        }
        if (job.State != JobState.Assigned)
        {
            return Results.BadRequest("Job is in wrong state.");
        }
        
        Job runningJob = job with { State = JobState.Running };
        if (!jobs.TryUpdate(jobId, runningJob, job))
        {
            return Results.BadRequest($"Job {jobId} already changed.");
        }
        return Results.Ok(runningJob);
    })
    .WithName("StartJob");
app.MapPost("/result", (JobResult res, ConcurrentDictionary<Guid, Job> jobs) =>
    {
        if (!jobs.TryGetValue(res.JobId, out Job job))
        {
            return Results.BadRequest("You're so mean for trying to hack us");            
        }
        if (job.State != JobState.Running)
        {
            return Results.BadRequest("Job is in wrong state.");
        }
        if (!res.Result)
        {
            Job failedJob = job with { State = JobState.Failed };
            if (!jobs.TryUpdate(res.JobId, failedJob, job))
            {
                return Results.BadRequest($"Job {res.JobId} already changed.");
            }    
            return Results.Ok(failedJob);
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
