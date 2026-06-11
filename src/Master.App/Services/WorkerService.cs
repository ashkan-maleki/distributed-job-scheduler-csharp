using Bogus;
using Master.Domain.Models;
using Master.Domain.Repositories;
using Master.Domain.Services;
using Shared.Domain.DTOs;

namespace Master.App.Services;

public class WorkerService(IWorkerRepository workerRepository, IDesiredStateRepository desiredStateRepository) : IWorkerService
{
    public async Task<List<Worker>> AllAsync() => await workerRepository.AllAsync();

   public async Task<Result<Worker>> RegisterAsync()
    {
        int currentNumberOfWorkers = await workerRepository.CountAsync();
        Result<DesiredState> schedulerStateResult = await desiredStateRepository.GetAsync();
        if (schedulerStateResult.NotFound)
        {
            return schedulerStateResult.NotFoundResult;
        }

        int desiredNumberOfWorkers = schedulerStateResult.OkResult.Value.DesiredNumberOfWorkers;
        if (currentNumberOfWorkers >= desiredNumberOfWorkers)
        {
            return new DomainFailure("We cannot register new workers now; wait.");
        }

        Faker newFaker = new Faker();
        string name = newFaker.Company.CompanyName()
            .ToLower()
            .Replace(" ", "-");
        Result<Worker> workerResult = await workerRepository.GetUnregisteredAsync();
        if (workerResult.NotFound)
        {
            return workerResult.NotFoundResult;
        }

        Worker worker = workerResult.OkResult.Value;
        worker.Register();

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
        IResult result = await ScaleDownAsync(count);
        if (result is NotFound notFound)
        {
            return notFound;
        }
        return await workerRepository.UnitOfWork.SaveEntitiesAsync();
    }

    public async Task<bool> CommitSuicideAsync(Guid workerId) => await workerRepository.AnyAsync(workerId);
}