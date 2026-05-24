namespace Worker.Rest.Services;

public interface IScopedExecutor
{
    Task Use<T>(
        Func<T, Task> action)
        where T : notnull;

    Task<TResult> Use<T, TResult>(
        Func<T, Task<TResult>> action)
        where T : notnull;
}

public class ScopedExecutor(
    IServiceScopeFactory scopeFactory)
    : IScopedExecutor
{
    public async Task Use<T>(
        Func<T, Task> action)
        where T : notnull
    {
        using IServiceScope scope =
            scopeFactory.CreateScope();

        T service =
            scope.ServiceProvider
                .GetRequiredService<T>();

        await action(service);
    }

    public async Task<TResult> Use<T, TResult>(
        Func<T, Task<TResult>> action)
        where T : notnull
    {
        using IServiceScope scope =
            scopeFactory.CreateScope();

        T service =
            scope.ServiceProvider
                .GetRequiredService<T>();

        return await action(service);
    }
}

public class ApplyScopeSample(IScopedExecutor scopedExecutor, ILogger<ApplyScopeSample> logger)
{
    public async Task A()
    {
        int count = await scopedExecutor.Use(async (IWorkerRepository repo) => { return await repo.CountAsync(); });
    }

    public async Task B()
    {
        int count = await scopedExecutor.Use<IWorkerRepository, int>(async repo
            =>
        {
            return await repo.CountAsync();
        });

        Console.WriteLine(count);
    }

    public async Task C()
    {
        await scopedExecutor.Use<IWorkerRepository>(async repo =>
        {
            int count = await repo.CountAsync();

            logger.LogInformation(
                "Workers: {count}",
                count);
        });
    }
}

public interface IWorkerRepository
{
    Task<int> CountAsync();
}