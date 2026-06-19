using Microsoft.EntityFrameworkCore;
using Shared.Domain.DTOs;
using Shared.Domain.EF;
using IResult = Shared.Domain.DTOs.IResult;

namespace Worker.Rest.EF;

public class WorkerDbContext(DbContextOptions<WorkerDbContext> options) 
    : DbContext(options), IUnitOfWork
{
    public DbSet<Domain.Worker> Workers { get; set; }
    public async Task<IResult> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _ = await SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException e)
        {
            return new CriticalError(e,"a concurrency violation is encountered while saving to the database.");
        }
        catch (DbUpdateException e)
        {
            return new CriticalError(e,"an error is encountered while saving to the database.");
        }

        return new Ok();
    }
}



// dotnet ef migrations add InitialCreate --project src/Worker.Rest --startup-project src/Worker.Rest --output-dir EF/Migrations
// dotnet ef database update --project src/Worker.Rest --startup-project src/Worker.Rest