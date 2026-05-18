using Shared.Domain.EF;

namespace Shared.Domain.Data;

public interface IRepository
{
    IUnitOfWork UnitOfWork { get; }
}

// public interface IRepository<T> where T : IAggregateRoot
// {
//     IUnitOfWork UnitOfWork { get; }
// }