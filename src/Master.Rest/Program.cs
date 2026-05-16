using Master.App.Services;
using Master.App.Stores;
using Master.Domain.Services;
using Master.Domain.Stores;
using Master.Rest.Apis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks();

builder.Services.AddSingleton<IWorkerStore, WorkerStore>();
builder.Services.AddSingleton<IJobStore, JobStore>();
builder.Services.AddScoped<IWorkerService, WorkerService>();
builder.Services.AddScoped<IJobService, JobService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGroup("/api").MapJobsApi().MapWorkersApi().MapHealthChecks("/hc");

app.Run();

public record JobRequest(string Name);
public record JobResult(Guid JobId, Guid WorkerId, bool Successful, string ErrorMessage);
