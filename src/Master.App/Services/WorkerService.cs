using Bogus;
using Master.Domain.Models;
using Master.Domain.Repositories;
using Master.Domain.Services;
using Shared.Domain.DTOs;

namespace Master.App.Services;

public class WorkerService(IWorkerRepository workerRepository, IWorkersStateRepository workersStateRepository)
    : IWorkerService
{
    public async Task<List<Worker>> AllAsync() => await workerRepository.AllAsync();

    public async Task<Result<Worker>> RegisterAsync()
    {
        Result<WorkersState> workersStateResult = await workersStateRepository.GetAsync();
        if (workersStateResult.NotFound)
            return workersStateResult.NotFoundResult;
        WorkersState workersState = workersStateResult.OkResult.Value;
        if (workersState.RegistrationNotAllowed)
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
            return criticalError;
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
                return criticalError;
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
        
        WorkersState workersState = new(count, await workerRepository.CountAsync());
        await workersStateRepository.AddAsync(workersState);

        IResult result = await workerRepository.UnitOfWork.SaveEntitiesAsync();
        if (result is CriticalError criticalError)
        {
            return criticalError;
        }

        return new Ok();
        
    }

    public async Task<bool> CommitSuicideAsync(Guid workerId) => await workerRepository.AnyAsync(workerId);
}