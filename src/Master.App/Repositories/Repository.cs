using Master.App.EF;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Data;
using Shared.Domain.EF;

namespace Master.App.Repositories;

public class Repository(
    SchedulerDbContext dbContext) : IRepository
{
    protected readonly SchedulerDbContext DbContext = dbContext;
    public IUnitOfWork UnitOfWork => DbContext;
}