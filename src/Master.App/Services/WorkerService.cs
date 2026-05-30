using Master.Domain.Models;
using Master.Domain.Repositories;
using Master.Domain.Services;
using Shared.Domain.DTOs;

namespace Master.App.Services;

public class WorkerService(IWorkerRepository workerRepository, SchedulerState schedulerState) : IWorkerService
{
    public async Task<List<Worker>> AllAsync() => await workerRepository.AllAsync();

    public async Task<Result2> ScaleAsync(int count)
    {
        return await Task.FromResult(Results.Ok());
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

    public async Task<QueryResult2<Worker>> RegisterAsync(string name)
    {
        int count = await workerRepository.CountAsync();
        // if (schedulerState.DesiredNumberOfWorkers >= schedulerState.CurrentNumberOfWorkers)
        // {
        //     return new(new WorkerServiceInternalError("Current number of workers is " + count), null);
        // }

        if (count >= schedulerState.DesiredNumberOfWorkers)
        {
            return QueryResults.DomainFailure<Worker>("We cannot register new workers now; wait.");
        }
        
        QueryResult2<Worker> workerQueryResult2 = await workerRepository.GetByNameAsync(name);
        Worker worker;
        if (workerQueryResult2.Found)
        {
            worker = workerQueryResult2.Data;
        }
        else
        {
            worker = new (name);
            _ = await workerRepository.AddAsync(worker);
        }
        
        worker.Register();
        Exception? exception = await workerRepository.UnitOfWork.SaveEntitiesAsync();
        if (exception is not null)
        {
            return QueryResults.ExceptionThrown<Worker>(exception);
        }
        return QueryResults.Found(worker);
    }

    public Task<Result2> UnregisterAsync(Worker worker)
    {
        throw new NotImplementedException();
    }

    public async Task<Result2> ReportHeartBeatAsync(Guid workerId)
    {
        QueryResult2<Worker> workerQueryResult2 = await workerRepository.GetAsync(workerId);
        if (workerQueryResult2.NotFound)
        {
            return workerQueryResult2;
        }
        Worker worker = workerQueryResult2.Data;
        worker.ReportHeartBeat();
        Exception? exception = await workerRepository.UnitOfWork.SaveEntitiesAsync();
        if (exception is not null)
        {
            return QueryResults.ExceptionThrown<Worker>(exception);
        }
        return QueryResults.Ok<Worker>();
    }
}