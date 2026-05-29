using Bogus;
using Master.Domain.Models;
using Master.Domain.Repositories;
using Master.Domain.Services;
using Shared.Domain.DTOs;

namespace Master.App.Services;

public class WorkerService(IWorkerRepository workerRepository, SchedulerState schedulerState) : IWorkerService
{
    public async Task<List<Worker>> AllAsync() => await workerRepository.AllAsync();

    public async Task<Result> ScaleAsync(int count)
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

    public async Task<QueryResult<Worker>> RegisterAsync(string name)
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
        
        QueryResult<Worker> workerQueryResult = await workerRepository.GetByNameAsync(name);
        Worker worker;
        if (workerQueryResult.Found)
        {
            worker = workerQueryResult.Data;
        }
        else
        {
            worker = new (name);
            _ = workerRepository.AddAsync(worker);
        }
        
        worker.Register();
        Exception? exception = await workerRepository.UnitOfWork.SaveEntitiesAsync();
        if (exception is not null)
        {
            return QueryResults.ExceptionThrown<Worker>(exception);
        }
        return QueryResults.Ok<Worker>();
    }

    public Task<Result> UnregisterAsync(Worker worker)
    {
        throw new NotImplementedException();
    }

    public async Task<Result> ReportHeartBeatAsync(Guid workerId)
    {
        QueryResult<Worker> workerQueryResult = await workerRepository.GetAsync(workerId);
        if (workerQueryResult.NotFound)
        {
            return workerQueryResult;
        }
        Worker worker = workerQueryResult.Data;
        worker.ReportHeartBeat();
        Exception? exception = await workerRepository.UnitOfWork.SaveEntitiesAsync();
        if (exception is not null)
        {
            return QueryResults.ExceptionThrown<Worker>(exception);
        }
        return QueryResults.Ok<Worker>();
    }
}