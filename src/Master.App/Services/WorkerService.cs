using Bogus;
using Master.Domain.Models;
using Master.Domain.Repositories;
using Master.Domain.Services;
using Shared.Domain.Failures;

namespace Master.App.Services;

public class WorkerService(IWorkerRepository workerRepository, SchedulerState schedulerState) : IWorkerService
{
    public async Task<List<Worker>> AllAsync() => await workerRepository.AllAsync();

    public async Task<IError?> ScaleAsync(int count)
    {
        IError? error = null;
        Faker faker = new();
        int workerCount = await workerRepository.CountAsync();
        for (int i = 0; i < count - workerCount; i++)
        {
            error =  await workerRepository.AddAsync(new Worker(faker.Company.CompanyName()));
            if (error != null)
            {
                return error;
            }
        }
        while (workerCount > count)
        {
            (error, Worker? worker) = await workerRepository.FirstAsync();
            if (error != null)
            {
                return error;
            }
            error = await workerRepository.RemoveAsync(worker!);
            if (error != null)
            {
                return error;
            }
        }
        error = await workerRepository.UnitOfWork.SaveEntitiesAsync();
        if (error != null)
        {
            return error;
        }
        return null;
    }

    public async Task<(IError?, Worker?)> RegisterAsync(string name)
    {
        // int count = await workerRepository.CountAsync();
        // if (schedulerState.DesiredNumberOfWorkers >= schedulerState.CurrentNumberOfWorkers)
        // {
        //     return new(new WorkerServiceInternalError("Current number of workers is " + count), null);
        // }

        if (schedulerState.CurrentNumberOfWorkers >= schedulerState.DesiredNumberOfWorkers)
        {
            return new(new WaitingSignalForWorkersError("We cannot register new workers now; wait."), null);
        }
        
        (IError? error, Worker? worker) = await workerRepository.GetByNameAsync(name);
        if (error != null)
        {
            worker = new (name);
            _ = workerRepository.AddAsync(worker);
        }
        schedulerState.CurrentNumberOfWorkers++;
        worker!.Register();
        
        return new(await workerRepository.UnitOfWork.SaveEntitiesAsync(), worker);
    }

    public Task<IError?> UnregisterAsync(Worker worker)
    {
        throw new NotImplementedException();
    }

    public async Task<IError?> ReportHeartBeatAsync(Guid workerId)
    {
        (IError? error, Worker? worker) = await workerRepository.GetAsync(workerId);
        if (error != null)
        {
            return error;
        }
        worker!.ReportHeartBeat();
        error = await workerRepository.UnitOfWork.SaveEntitiesAsync();
        return error;
    }
}