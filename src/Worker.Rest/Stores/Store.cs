using Shared.Domain.Data;
using Shared.Domain.EF;
using Worker.Rest.EF;

namespace Worker.Rest.Stores;


public class Store(WorkerDbContext dbContext) : IRepository
{
    protected readonly WorkerDbContext DbContext = dbContext;
    public IUnitOfWork UnitOfWork => DbContext;
}