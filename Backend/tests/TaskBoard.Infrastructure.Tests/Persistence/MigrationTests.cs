using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TaskBoard.Domain.Entities;
using TaskBoard.Domain.Enums;
using TaskBoard.Infrastructure.Persistence;
using TaskBoard.Infrastructure.Persistence.Seeding;

namespace TaskBoard.Infrastructure.Tests.Persistence;

/// <summary>Runs the real migrations against a temporary SQLite <b>file</b>, simulating application restarts.</summary>
public sealed class MigrationTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"taskboard-migrations-{Guid.NewGuid():N}.db");

    private TaskBoardDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<TaskBoardDbContext>().UseSqlite($"Data Source={_path}").Options);

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        File.Delete(_path);
    }

    [Fact]
    public async Task Migrations_match_the_model()
    {
        await using var context = CreateContext();

        await context.Database.MigrateAsync();

        Assert.False(context.Database.HasPendingModelChanges(), "Run 'dotnet ef migrations add' — the model has changes not captured in a migration.");
        Assert.Empty(await context.Database.GetPendingMigrationsAsync());
    }

    [Fact]
    public async Task Data_survives_restarts_and_the_seed_is_applied_only_once()
    {
        var created = TaskItem.Create(SeedData.AliceId, "Created before restart", null, DateTime.UtcNow.AddDays(1));
        Guid editedSeedTaskId;
        int seededTaskCount;
        await using (var firstRun = CreateContext())
        {
            await firstRun.Database.MigrateAsync();
            seededTaskCount = await firstRun.TaskItems.CountAsync();
            firstRun.TaskItems.Add(created);
            var seededTask = await firstRun.TaskItems.FirstAsync(t => t.UserId == SeedData.BobId && t.Status != TaskItemStatus.Done);
            seededTask.ChangeStatus(TaskItemStatus.Done); // a user change to seeded data
            editedSeedTaskId = seededTask.Id;
            await firstRun.SaveChangesAsync();
        }

        SqliteConnection.ClearAllPools();

        await using var secondRun = CreateContext();
        await secondRun.Database.MigrateAsync(); // what Development start-up does on every restart

        Assert.True(seededTaskCount > 0, "the seed should create tasks");
        Assert.Equal(2, await secondRun.Users.CountAsync());
        Assert.Equal(seededTaskCount + 1, await secondRun.TaskItems.CountAsync());
        Assert.True(await secondRun.TaskItems.AnyAsync(t => t.Id == created.Id));
        Assert.Equal(TaskItemStatus.Done, (await secondRun.TaskItems.SingleAsync(t => t.Id == editedSeedTaskId)).Status);
    }
}
