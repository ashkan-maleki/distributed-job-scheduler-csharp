using Master.Domain.Models;
using Master.Domain.Stores;
using Shared.Domain.Failures;

namespace Master.App.Stores;

public class WorkerStore : IWorkerStore
{
    private readonly Dictionary<Guid, Worker> _workers = new();

    public List<Worker> Workers => _workers.Values.ToList();
    public int WorkersCount => _workers.Count;

    public IError? TryAddWorker(Worker worker)
    {
        if (!_workers.TryAdd(worker.Id, worker))
        {
            return new WorkerStoreOperationError($"A worker with the same Id ({worker.Id}) has already been added.");
        }

        return null;
    }

    public IError? TryRemoveWorker(Worker worker)
    {
        if (!_workers.Remove(worker.Id))
        {
            return new WorkerStoreOperationError($"The worker with this Id ({worker.Id}) did not exist.");
        }

        return null;
    }

    public IError? TryAddWorkerRange(List<Worker> workers)
    {
        foreach (var worker in workers)
        {
            IError? error = TryAddWorker(worker);
            if (error != null)
            {
                return error;
            }
        }

        return null;
    }

    public (IError?, Worker?) TryGetWorker(Guid workerId)
    {
        if (!_workers.TryGetValue(workerId, out var worker))
        {
            return new(new WorkerStoreNotFoundError($"There is no worker with id ({workerId}) in the list"), null);
        }

        return (null, worker);
    }


    public (IError?, Worker?) TryFirstWorker()
    {
        KeyValuePair<Guid, Worker>? workerPair = _workers.FirstOrDefault();
        if (workerPair is null)
        {
            return new(new WorkerStoreNotFoundError($"There is no worker in the list"), null);
        }

        return new(null, workerPair.Value.Value);
    }
}