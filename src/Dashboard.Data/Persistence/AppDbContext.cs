using Dashboard.Data.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dashboard.Data.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<TodoEntity> Todos => Set<TodoEntity>();
    public DbSet<JobApplicationEntity> JobApplications => Set<JobApplicationEntity>();
    public DbSet<JournalEntryEntity> JournalEntries => Set<JournalEntryEntity>();
    public DbSet<HealthReadingEntity> HealthReadings => Set<HealthReadingEntity>();
    public DbSet<SyncCursorEntity> SyncCursors => Set<SyncCursorEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
