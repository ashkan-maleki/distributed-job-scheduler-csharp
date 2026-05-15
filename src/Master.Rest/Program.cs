using Master.App.Stores;
using Master.Domain.Aggregates;
using Master.Domain.Stores;
using Shared.Domain.Failures;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IJobStore, JobStore>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/job", (JobRequest req, IJobStore jobStore) =>
    {
        (IError? error, Job? job) = jobStore.TryQueueJob(req.Name);
        if (error is not null)
        {
            return Results.BadRequest(error.ToString());
        }
        return Results.Ok(job);
    })
    .WithName("SaveJob");
app.MapGet("/job", (Guid workerId, IJobStore jobStore) =>
    {
        (IError? error, Job? job) = jobStore.TryAssignJob(workerId);
        if (error is not null && error.Is<JobStoreNotFoundError>()) 
        {
            return Results.NotFound(error.ToString());
        }

        if (error is not null && (error.As<Job>() || error.Is<JobStoreOperationError>()))
        {
            return Results.BadRequest(error.ToString());
        }
        return Results.Ok(job);
    })
    .WithName("GetJob");
app.MapPost("job/start", (Guid jobId, Guid workerId, IJobStore jobStore) =>
    {
        (IError? error, Job? job) = jobStore.TryStartJob(jobId, workerId);
        if (error is not null && error.Is<JobStoreNotFoundError>()) 
        {
            return Results.NotFound(error.ToString());
        }

        if (error is not null && (error.As<Job>() || error.Is<JobStoreOperationError>()))
        {
            return Results.BadRequest(error.ToString());
        }
        return Results.Ok(job);
    })
    .WithName("StartJob");
app.MapPost("job/result", (JobResult res, IJobStore jobStore) =>
    {
        IError? err = null;
        Job? job = null;
        
        if (!res.Successful)
        {
            (err, job) = jobStore.TryFailJob(res.JobId, res.WorkerId);
        }
        else
        {
            (err, job) = jobStore.TryCompleteJob(res.JobId, res.WorkerId);
        }
        
        if (err is not null && err.Is<JobStoreNotFoundError>()) 
        {
            return Results.NotFound(err.ToString());
        }

        if (err is not null && (err.As<Job>() || err.Is<JobStoreOperationError>()))
        {
            return Results.BadRequest(err.ToString());
        }
        return Results.Ok(job);
        
    })
    .WithName("SaveResult");

app.Run();

public record JobRequest(string Name);
public record JobResult(Guid JobId, Guid WorkerId, bool Successful, string ErrorMessage);
