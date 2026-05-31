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

    public async Task<IResult<Worker>> RegisterAsync(string name)
    {
        int count = await workerRepository.CountAsync();
        // if (schedulerState.DesiredNumberOfWorkers >= schedulerState.CurrentNumberOfWorkers)
        // {
        //     return new(new WorkerServiceInternalError("Current number of workers is " + count), null);
        // }

        if (count >= schedulerState.DesiredNumberOfWorkers)
        {
            return new Error<Worker>("We cannot register new workers now; wait.");
        }
        
        IResult result = await workerRepository.GetByNameAsync(name);
        Worker worker;
        if (result is Ok<Worker> ok)
        {
             worker = ok.Value;
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
            return new CriticalError<Worker>(criticalError.Message);
        }

        return new Ok<Worker>(worker);
    }

    public Task<IResult> UnregisterAsync(Worker worker)
    {
        throw new NotImplementedException();
    }

    public async Task<IResult> ReportHeartBeatAsync(Guid workerId)
    {
        IResult result = await workerRepository.GetAsync(workerId);
        if (result is NotFound<Worker> notFound)
        {
            return notFound;
        }
        
        if (result is not Ok<Worker> ok)
        {
            return new UnknownError<Worker>("error occured while reporting heartbeat.");
        }
        
        Worker worker = ok.Value;
        worker.ReportHeartBeat();
        result = await workerRepository.UnitOfWork.SaveEntitiesAsync();

        if (result is CriticalError criticalError)
        {
            return new CriticalError<Worker>(criticalError.Message);
        }

        return new Ok<Worker>(worker);
    }
}