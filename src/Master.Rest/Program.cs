using MassTransit;
using Master.App.EF;
using Master.App.Repositories;
using Master.App.Services;
using Master.Domain.Models;
using Master.Domain.Repositories;
using Master.Domain.Services;
using Master.Rest.Apis;
using Master.Rest.BackgroundServices;
using Master.Rest.Consumers;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks();

builder.Services.AddDbContext<SchedulerDbContext>(options => { options.UseSqlite("Data Source=scheduler.db"); });
builder.Services.AddScoped<IWorkersStateRepository, WorkersStateRepository>();
builder.Services.AddScoped<IDesiredStateRepository, DesiredStateRepository>();
builder.Services.AddScoped<IWorkerRepository, WorkerRepository>();
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IDesiredStateService, DesiredStateService>();
builder.Services.AddScoped<IWorkerService, WorkerService>();
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddSingleton<IConcurrentRegistrationService, ConcurrentRegistrationService>();

// builder.Services.AddHostedService<WorkersCountBackgroundService>();
builder.Services.AddMassTransit(x => 
{
    x.UsingInMemory((ctx, cfg) => cfg.ConfigureEndpoints(ctx));
    x.AddConsumer<DesiredStateConsumer>();
});
// builder.Services.AddMassTransitHostedService();


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
app.MapGroup("/api/scheduler-states").MapSchedulerStatesApi();

app.Run();


