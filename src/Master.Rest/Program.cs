using Master.App.EF;
using Master.App.Repositories;
using Master.App.Services;
using Master.Domain.Models;
using Master.Domain.Repositories;
using Master.Domain.Services;
using Master.Rest.Apis;
using Master.Rest.BackgroundServices;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks();

SchedulerState state = new();
builder.Services.AddSingleton(state);
builder.Services.AddDbContext<SchedulerDbContext>(options => { options.UseSqlite("Data Source=scheduler.db"); });
builder.Services.AddScoped<IWorkerRepository, WorkerRepository>();
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IWorkerService, WorkerService>();
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddHostedService<WorkersCountBackgroundService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGroup("/api").MapJobsApi().MapWorkersApi().MapSchedulerStatesApi().MapHealthChecks("/hc");

app.Run();


