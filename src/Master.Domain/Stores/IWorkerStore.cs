using Master.Domain.Models;
using Shared.Domain.Failures;

namespace Master.Domain.Stores;

public interface IWorkerStore
{
    public List<Worker> Workers { get; }
    public int WorkersCount { get; }

    public IError? TryAddWorker(Worker worker);
    public IError? TryRemoveWorker(Worker worker);
    
    public IError? TryAddWorkerRange(List<Worker> workers);
    public (IError?, Worker?) TryGetWorker(Guid workerId);
    public (IError?, Worker?) TryFirstWorker();
}

public class WorkerStoreOperationError(string message) : Error<IWorkerStore>(message);
public class WorkerStoreNotFoundError(string message) : Error<IWorkerStore>(message);