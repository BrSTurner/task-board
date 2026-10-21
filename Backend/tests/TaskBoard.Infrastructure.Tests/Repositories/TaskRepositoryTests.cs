using Microsoft.EntityFrameworkCore;
using TaskBoard.Domain.Entities;
using TaskBoard.Domain.Enums;
using TaskBoard.Infrastructure.Persistence.Repositories;

namespace TaskBoard.Infrastructure.Tests.Repositories;

public sealed class TaskRepositoryTests : IDisposable
{
    private static readonly DateTime DueUtc = new(2026, 10, 21, 18, 0, 0, DateTimeKind.Utc);
    private readonly SqliteTestDatabase _database = new();

    public void Dispose() => _database.Dispose();

    private async Task<User> AddUserAsync(string email)
    {
        var user = User.Create("Owner", email, "UTC", "hash");
        await using var context = _database.CreateContext();
        Assert.True(await new UserRepository(context).TryAddAsync(user, CancellationToken.None));
        return user;
    }

    private async Task<TaskItem> AddTaskAsync(User owner, string title)
    {
        var task = TaskItem.Create(owner.Id, title, "desc", DueUtc);
        await using var context = _database.CreateContext();
        await new TaskRepository(context).AddAsync(task, CancellationToken.None);
        return task;
    }

    [Fact]
    public async Task Added_task_round_trips_with_utc_due_date()
    {
        var owner = await AddUserAsync("owner@example.test");
        var task = await AddTaskAsync(owner, "Persisted");

        await using var context = _database.CreateContext();
        var loaded = Assert.Single(await new TaskRepository(context).ListByUserAsync(owner.Id, CancellationToken.None));

        Assert.Equal(task.Id, loaded.Id);
        Assert.Equal("Persisted", loaded.Title);
        Assert.Equal("desc", loaded.Description);
        Assert.Equal(TaskItemStatus.Todo, loaded.Status);
        Assert.Equal(DueUtc, loaded.DueDateUtc);
        // SQLite has no DateTime type; without a converter the Kind would come back Unspecified.
        Assert.Equal(DateTimeKind.Utc, loaded.DueDateUtc.Kind);
    }

    [Fact]
    public async Task Queries_are_scoped_to_the_owner()
    {
        var alice = await AddUserAsync("alice.repo@example.test");
        var bob = await AddUserAsync("bob.repo@example.test");
        var bobsTask = await AddTaskAsync(bob, "Bob only");
        await AddTaskAsync(alice, "Alice only");

        await using var context = _database.CreateContext();
        var repository = new TaskRepository(context);

        Assert.Equal(["Alice only"], (await repository.ListByUserAsync(alice.Id, CancellationToken.None)).Select(t => t.Title));
        Assert.Null(await repository.GetForUserAsync(bobsTask.Id, alice.Id, CancellationToken.None));
        Assert.NotNull(await repository.GetForUserAsync(bobsTask.Id, bob.Id, CancellationToken.None));
    }

    [Fact]
    public async Task Update_and_delete_are_persisted()
    {
        var owner = await AddUserAsync("owner@example.test");
        var task = await AddTaskAsync(owner, "Original");

        await using (var context = _database.CreateContext())
        {
            var repository = new TaskRepository(context);
            var tracked = await repository.GetForUserAsync(task.Id, owner.Id, CancellationToken.None);
            tracked!.UpdateDetails("Edited", null, DueUtc.AddDays(1), TaskItemStatus.InProgress);
            await repository.UpdateAsync(tracked, CancellationToken.None);
        }

        await using (var context = _database.CreateContext())
        {
            var reloaded = await new TaskRepository(context).GetForUserAsync(task.Id, owner.Id, CancellationToken.None);
            Assert.Equal("Edited", reloaded!.Title);
            Assert.Null(reloaded.Description);
            Assert.Equal(TaskItemStatus.InProgress, reloaded.Status);
            await new TaskRepository(context).DeleteAsync(reloaded, CancellationToken.None);
        }

        await using var verify = _database.CreateContext();
        Assert.Null(await new TaskRepository(verify).GetForUserAsync(task.Id, owner.Id, CancellationToken.None));
    }

    [Fact]
    public async Task Status_is_stored_as_readable_text()
    {
        var owner = await AddUserAsync("owner@example.test");
        var task = await AddTaskAsync(owner, "Text status");

        await using var context = _database.CreateContext();
        var status = await context.Database
            .SqlQuery<string>($"SELECT Status AS Value FROM TaskItems WHERE Id = {task.Id}")
            .SingleAsync();

        Assert.Equal("Todo", status);
    }

    [Fact]
    public async Task Foreign_key_rejects_a_task_for_an_unknown_user()
    {
        var orphan = TaskItem.Create(Guid.NewGuid(), "Orphan", null, DueUtc);

        await using var context = _database.CreateContext();

        await Assert.ThrowsAsync<DbUpdateException>(() => new TaskRepository(context).AddAsync(orphan, CancellationToken.None));
    }

    [Fact]
    public async Task Deleting_a_user_that_still_has_tasks_is_restricted()
    {
        var owner = await AddUserAsync("owner@example.test");
        await AddTaskAsync(owner, "Keeps the user alive");

        await using var context = _database.CreateContext();
        context.Users.Remove(await context.Users.SingleAsync(u => u.Id == owner.Id));

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }
}
