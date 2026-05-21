namespace Worker.Rest.Works;

public static class SimpleWork
{
    public static async Task Run(CancellationToken stoppingToken, int coefficient = 1)
    {
        int rand = Random.Shared.Next(1,3);
        rand *= coefficient;
        TimeSpan seconds = TimeSpan.FromSeconds(rand);
        await Task.Delay(seconds, stoppingToken);
    }
}