using Shared.Domain.DTOs;

namespace Shared.Domain.EF;

public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<Exception?> SaveEntitiesAsync(CancellationToken cancellationToken = default);
}

