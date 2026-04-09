using Microsoft.EntityFrameworkCore;
using RVM.LogStream.Domain.Entities;

namespace RVM.LogStream.Infrastructure.Data;

public class LogStreamDbContext(DbContextOptions<LogStreamDbContext> options) : DbContext(options)
{
    public DbSet<LogEntry> LogEntries => Set<LogEntry>();
    public DbSet<LogSource> LogSources => Set<LogSource>();
    public DbSet<RetentionPolicy> RetentionPolicies => Set<RetentionPolicy>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LogStreamDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<RetentionPolicy>()
            .Where(e => e.State == EntityState.Modified))
        {
            entry.Entity.UpdatedAt = DateTime.UtcNow;
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
