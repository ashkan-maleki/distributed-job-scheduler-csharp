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

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}