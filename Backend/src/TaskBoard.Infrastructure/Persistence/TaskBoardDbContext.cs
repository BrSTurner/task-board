using Microsoft.EntityFrameworkCore;
using TaskBoard.Domain.Entities;

namespace TaskBoard.Infrastructure.Persistence;

public sealed class TaskBoardDbContext(DbContextOptions<TaskBoardDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TaskBoardDbContext).Assembly);
}
