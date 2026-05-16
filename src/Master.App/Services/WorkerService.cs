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
            for (int i = 0; i < workerStore.WorkersCount - count; i++)
            {
                workers.Add(new Worker("hi"));
            }
            
            IError? err = workerStore.TryAddWorkerRange(workers);
            if (err != null)
            {
                return err;
            }
        }
        while (workerStore.WorkersCount > count)
        {
            (IError? err, Worker? worker) = workerStore.TryFirstWorker();
            if (err != null)
            {
                return err;
            }
            err = workerStore.TryRemoveWorker(worker!);
            if (err != null)
            {
                return err;
            }
        }
        return null;
    }
}