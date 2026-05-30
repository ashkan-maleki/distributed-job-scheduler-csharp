using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Worker.Rest.BackgroundServices;
using Worker.Rest.Config;
using Worker.Rest.Contexts;
using Worker.Rest.EF;
using Worker.Rest.HttpServices;
using Worker.Rest.HttpServices.Master;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
ConcurrentDictionary<int, Worker.Rest.Domain.Worker> workers = new();
WorkerContext context = new();

builder.Services.ConfigureOptions<ApiConfigSetup>();
// builder.Services.AddDbContext<WorkerDbContext>(options => {options.UseSqlite("Data Source=scheduler.db");});
builder.Services.AddDbContextFactory<WorkerDbContext>(options => { options.UseSqlite("Data Source=scheduler.db"); });
builder.Services.AddMasterHttpClients();
builder.Services.AddSingleton(context);

builder.Services.AddOpenApi();

builder.Services.AddHostedService<MasterHealthCheckBackgroundService>()
    .AddHostedService<RegistrationBackgroundService>()
    // .AddHostedService<HeartBeatBackgroundService>()
    // .AddHostedService<JobBackgroundService>()
    ;
// builder.Services.AddHostedService<SimpleWorker>();
// builder.Services.AddHostedService<ParallelWorker>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


app.Run();


// for (int i = 0; i < 5; i++)
// {
//     Task.Run(() => WorkerLoop());
// }
//
// Random random = new();
//
// int seconds =
//     Random.Shared.Next(60, 301);;
//
// await Task.Delay(
//     TimeSpan.FromSeconds(seconds));
//
//
// public interface IJobExecutor
// {
//     Task ExecuteAsync(
//         Job job,
//         CancellationToken ct);
// }
//
// public class FakeJobExecutor
//     : IJobExecutor
// {
//     private readonly Random _random = new();
//
//     public async Task ExecuteAsync(
//         Job job,
//         CancellationToken ct)
//     {
//         int duration =
//             _random.Next(60, 301);
//
//         Console.WriteLine(
//             $"Executing {job.Name} for {duration} seconds");
//
//         await Task.Delay(
//             TimeSpan.FromSeconds(duration),
//             ct);
//     }
// }