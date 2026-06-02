using Master.Domain.Models;
using Master.Domain.Repositories;
using Master.Domain.Services;
using Shared.Domain.DTOs;

namespace Master.App.Services;

public class WorkerService(IWorkerRepository workerRepository, SchedulerState schedulerState) : IWorkerService
{
    public async Task<List<Worker>> AllAsync() => await workerRepository.AllAsync();

    public async Task<IResult> ScaleAsync(int count)
    {
        return await Task.FromResult(new Ok());
        // Result error = null;
        // Faker faker = new();
        // int workerCount = await workerRepository.CountAsync();
        // for (int i = 0; i < count - workerCount; i++)
        // {
        //     error =  await workerRepository.AddAsync(new Worker(faker.Company.CompanyName()));
        //     if (error != null)
        //     {
        //         return error;
        //     }
        // }
        // while (workerCount > count)
        // {
        //     (error, Worker? worker) = await workerRepository.FirstAsync();
        //     if (error != null)
        //     {
        //         return error;
        //     }
        //     error = await workerRepository.RemoveAsync(worker!);
        //     if (error != null)
        //     {
        //         return error;
        //     }
        // }
        // error = await workerRepository.UnitOfWork.SaveEntitiesAsync();
        // if (error != null)
        // {
        //     return error;
        // }
        // return null;
    }

    public async Task<Result<Worker>> RegisterAsync(string name)
    {
        int count = await workerRepository.CountAsync();
        // if (schedulerState.DesiredNumberOfWorkers >= schedulerState.CurrentNumberOfWorkers)
        // {
        //     return new(new WorkerServiceInternalError("Current number of workers is " + count), null);
        // }

        if (count >= schedulerState.DesiredNumberOfWorkers)
        {
            return new Error("We cannot register new workers now; wait.");
        }
        
        IResult result = await workerRepository.GetByNameAsync(name);
        Worker worker;
        if (result is Object<Worker> objectResult)
        {
             worker = objectResult.Value;
        }
        else
        {
            worker = new (name);
            await workerRepository.AddAsync(worker);
        }
        
        worker.Register();
        result = await workerRepository.UnitOfWork.SaveEntitiesAsync();

        if (result is CriticalError criticalError)
        {
            return new CriticalError(criticalError.Message);
        }

        return worker;
    }

    public Task<IResult> UnregisterAsync(Worker worker)
    {
        throw new NotImplementedException();
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
}