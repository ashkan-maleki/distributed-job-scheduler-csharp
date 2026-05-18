using Master.Domain.Aggregates;
using Master.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.EF;
using Shared.Domain.Failures;

namespace Master.App.EF;

public class SchedulerDbContext(DbContextOptions<SchedulerDbContext> options) 
    : DbContext(options), IUnitOfWork
{
    public DbSet<Job> Jobs { get; set; }
    public DbSet<Worker> Workers { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Job>()
            .HasOne<Worker>()
            .WithMany()
            .HasForeignKey(j => j.WorkerId);
        
        modelBuilder.Entity<Job>()
            .Property(j => j.Version)
            .IsConcurrencyToken();
        base.OnModelCreating(modelBuilder);
    }

    public async Task<IError?> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        _ = await SaveChangesAsync(cancellationToken);
        return null;
    }
}


// dotnet add package Microsoft.EntityFrameworkCore
// dotnet add package Microsoft.EntityFrameworkCore.Sqlite
//  dotnet add package Microsoft.EntityFrameworkCore.Design

// Global tool:
// dotnet tool install --global dotnet-ef
// Verify:
// dotnet ef

// dotnet ef migrations add InitialCreate --project src/Master.App --startup-project src/Master.Rest --output-dir EF/Migrations
// dotnet ef database update --project src/Master.App --startup-project src/Master.Rest

// [Timestamp]
// public byte[] Version { get; set; }