using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TaskBoard.Domain.Entities;

namespace TaskBoard.Infrastructure.Persistence;

public sealed class TaskBoardDbContext(DbContextOptions<TaskBoardDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<TaskItem> TaskItems => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TaskBoardDbContext).Assembly);

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
        // SQLite stores DateTime as text and loses DateTimeKind; every DateTime in this model is UTC.
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();

    private sealed class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
        toProvider => toProvider,
        fromProvider => DateTime.SpecifyKind(fromProvider, DateTimeKind.Utc));
}
