using Bogus;
using Master.Domain.Models;
using Master.Domain.Repositories;
using Master.Domain.Services;
using Shared.Domain.DTOs;

namespace Master.App.Services;

public class WorkerService(IWorkerRepository workerRepository) : IWorkerService
{
    public async Task<List<Worker>> AllAsync() => await workerRepository.AllAsync();

   public async Task<Result<Worker>> RegisterAsync()
    {
        int currentNumberOfWorkers = await workerRepository.CountAsync();
        
        WorkersState workersState = new(currentNumberOfWorkers, await workerRepository.CountAsync());
        if (workersState.RegistrationAllowed)
        {
            return new DomainFailure("We cannot register new workers now; wait.");
        }
        
        Result<Worker> workerResult = await workerRepository.GetUnregisteredAsync();
        if (workerResult.NotFound)
        {
            return workerResult.NotFoundResult;
        }

        Worker worker = workerResult.OkResult.Value;
        worker.Register();
        workersState.Register();
        
        IResult result = await workerRepository.UnitOfWork.SaveEntitiesAsync();
        if (result is CriticalError criticalError)
        {
            return new CriticalError(criticalError.Message);
        }

        return worker;
    }

    public async Task<IResult> ReportHeartBeatAsync(Guid workerId)
    {
        Result<Worker> workerResult = await workerRepository.GetAsync(workerId);
        if (workerResult.NotFound)
        {
            return workerResult.NotFoundResult;
        }

        if (workerResult.TryGetValue(out Worker? worker))
        {
            worker.ReportHeartBeat();
            IResult result = await workerRepository.UnitOfWork.SaveEntitiesAsync();

            if (result is CriticalError criticalError)
            {
                return new CriticalError(criticalError.Message);
            }
        }

        return new Ok();
    }
    
    private async Task ScaleUpAsync(int count)
    {
        Faker faker = new();
        for (int i = await workerRepository.CountAsync(); i < count; i++)
        {
            string name = faker.Company.CompanyName()
                .ToLower()
                .Replace(" ", "-");
            await workerRepository.AddAsync(new Worker(name));
        }
    }
    
    private async Task<IResult> ScaleDownAsync(int count)
    {
        for (int i = await workerRepository.CountAsync(); i > count; i--)
        {
            Result<Worker> result = await workerRepository.FirstAsync();
            if (result.NotFound)
            {
                return result.NotFoundResult;
            }
            workerRepository.Remove(result.OkResult.Value);
        }

        return new Ok();
    }

    public async Task<IResult> ScaleAsync(int count)
    {
        await ScaleUpAsync(count);


        IResult result = await workerRepository.UnitOfWork.SaveEntitiesAsync();
        if (result is CriticalError criticalError)
        {
            return criticalError;
        }
        WorkersState workersState = new(count, await workerRepository.CountAsync());
        return new Ok();
    }

    public async Task<bool> CommitSuicideAsync(Guid workerId) => await workerRepository.AnyAsync(workerId);
}