using Shared.Domain.Failures;

namespace Shared.Domain.EF;

public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<IError?> SaveEntitiesAsync(CancellationToken cancellationToken = default);
}

public class SaveOperationError(string message) : Error<IUnitOfWork>(message);