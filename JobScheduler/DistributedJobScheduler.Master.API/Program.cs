using System.Collections.Concurrent;
using DistributedJobScheduler.Shared;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

ConcurrentQueue<Job> jobs = new ConcurrentQueue<Job>();
jobs.Enqueue(new Job("Job 1: Wash dishes"));
jobs.Enqueue(new Job("Job 2: Clean your room"));
jobs.Enqueue(new Job("Job 3: Work on the garden"));

app.MapPost("/job", (JobRequest req) =>
    {
        Job job = new Job(req.Name);
        jobs.Enqueue(job);
        return job.Id;
    })
    .WithName("SaveJob");
app.MapGet("/job", () =>
    {
        if (jobs.TryDequeue(out var job)) 
        {
            return Results.Ok(job);
        }

        return Results.NoContent();
    })
    .WithName("GetJob");
app.MapPost("/result", (JobResult res) =>
    {
        if (res.Result)
        {
            return "Thank you for your loyal service.";
        }

        return "You're a useless worker.";
    })
    .WithName("SaveResult");

app.Run();



record JobRequest(string Name);
record JobResult(Guid JobId, bool Result, string ErrorMessage);
