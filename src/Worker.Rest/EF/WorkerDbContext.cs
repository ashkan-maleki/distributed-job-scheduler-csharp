using Microsoft.EntityFrameworkCore;

namespace Worker.Rest.EF;

public class WorkerDbContext(DbContextOptions<WorkerDbContext> options) 
    : DbContext(options)
{
    public DbSet<Domain.Worker> Workers { get; set; }
}



// dotnet ef migrations add InitialCreate --project src/Worker.Rest --startup-project src/Worker.Rest --output-dir EF/Migrations
// dotnet ef database update --project src/Worker.Rest --startup-project src/Worker.Rest