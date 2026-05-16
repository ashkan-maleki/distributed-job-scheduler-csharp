using Bogus;
using Master.Domain.Models;
using Master.Domain.Services;
using Master.Domain.Stores;
using Shared.Domain.Failures;

namespace Master.App.Services;

public class WorkerService(IWorkerStore workerStore) : IWorkerService
{
    public List<Worker> Workers => workerStore.Workers;
    public IError? TryScale(int count)
    {
        if (workerStore.WorkersCount < count)
        {
            List<Worker> workers = new();
            Faker faker = new();
            for (int i = 0; i < count - workerStore.WorkersCount; i++)
            {
                workers.Add(new Worker(faker.Company.CompanyName()));
            }
            
            IError? error = workerStore.TryAddWorkerRange(workers);
            if (error != null)
            {
                return error;
            }
        }
        while (workerStore.WorkersCount > count)
        {
            (IError? error, Worker? worker) = workerStore.TryFirstWorker();
            if (error != null)
            {
                return error;
            }
            error = workerStore.TryRemoveWorker(worker!);
            if (error != null)
            {
                return error;
            }
        }
        return null;
    }
}