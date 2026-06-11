using Master.Domain.Aggregates;
using Master.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Domain.DTOs;
using Shared.Domain.EF;

namespace Master.App.EF;

public class SchedulerDbContext(DbContextOptions<SchedulerDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<Job> Jobs { get; set; }
    public DbSet<Worker> Workers { get; set; }
    public DbSet<DesiredState> SchedulerStates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Job>()
            .HasOne<Worker>()
            .WithMany()
            .HasForeignKey(j => j.WorkerId);

        modelBuilder.Entity<Job>()
            .Property(j => j.Version)
            .IsConcurrencyToken();

        modelBuilder.Entity<Worker>()
            .Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(200);

        modelBuilder.Entity<Worker>()
            .HasIndex(w => w.Name)
            .IsUnique();
        base.OnModelCreating(modelBuilder);
    }

    public async Task<IResult> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _ = await SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException e)
        {
            return new CriticalError("a concurrency violation is encountered while saving to the database.");
        }
        catch (DbUpdateException e)
        {
            return new CriticalError("an error is encountered while saving to the database.");
        }

        return new Ok();
    }
}


// dotnet add package Microsoft.EntityFrameworkCore
// dotnet add package Microsoft.EntityFrameworkCore.Sqlite
//  dotnet add package Microsoft.EntityFrameworkCore.Design

// Global tool:
// dotnet tool install --global dotnet-ef
// Verify:
// dotnet ef

// dotnet ef migrations add AddDesiredState --project src/Master.App --startup-project src/Master.Rest --output-dir EF/Migrations
// dotnet ef database update --project src/Master.App --startup-project src/Master.Rest

// [Timestamp]
// public byte[] Version { get; set; }