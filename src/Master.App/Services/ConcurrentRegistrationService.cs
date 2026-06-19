using Master.Domain.Models;
using Master.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Retry;
using Shared.Domain.DTOs;

namespace Master.App.Services;

public class ConcurrentRegistrationService(IServiceScopeFactory scopeFactory) : IConcurrentRegistrationService
{
    public async Task<Result<Worker>> RegisterAsync()
    {
        AsyncRetryPolicy<Result<Worker>> registerRetryPolicy =
            Policy<Result<Worker>>.HandleResult(r =>
                {
                    if (!r.CriticalErrorRaised)
                    {
                        return false;
                    }
                    return r.CriticalErrorResult.Exception is DbUpdateConcurrencyException;
                })
                .WaitAndRetryAsync(5, retryAttempt => 
                    TimeSpan.FromMilliseconds(Random.Shared.Next(10, 50)));
        
        
        return await registerRetryPolicy.ExecuteAsync(async () =>
        {
            using IServiceScope scope = scopeFactory.CreateScope();

            var workerService = scope.ServiceProvider.GetRequiredService<IWorkerService>();

            return await workerService.RegisterAsync();
        });
    }
}