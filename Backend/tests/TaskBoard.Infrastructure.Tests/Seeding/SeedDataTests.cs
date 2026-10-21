using Microsoft.EntityFrameworkCore;
using TaskBoard.Domain.Entities;
using TaskBoard.Domain.Enums;
using TaskBoard.Infrastructure.Authentication;
using TaskBoard.Infrastructure.Persistence.Seeding;

namespace TaskBoard.Infrastructure.Tests.Seeding;

public sealed class SeedDataTests : IDisposable
{
    private static readonly DateTime SeedReferenceDate = new(2026, 10, 21, 0, 0, 0, DateTimeKind.Utc);
    private readonly SqliteTestDatabase _database = new();

    public void Dispose() => _database.Dispose();

    [Fact]
    public async Task Two_demo_users_are_seeded_with_normalized_emails_and_iana_timezones()
    {
        await using var context = _database.CreateContext();

        var users = await context.Users.AsNoTracking().Where(u => u.Id == SeedData.AliceId || u.Id == SeedData.BobId)
            .OrderBy(u => u.Email).ToListAsync();

        Assert.Equal(["bob@example.test", "demo@example.test"], users.Select(u => u.Email));
        Assert.Equal(["Asia/Tokyo", "America/Sao_Paulo"], users.Select(u => u.TimeZone));
        Assert.All(users, u => Assert.Equal(User.NormalizeEmail(u.Email), u.Email));
        Assert.All(users, u => Assert.InRange(u.Name.Length, 1, User.NameMaxLength));
    }

    [Fact]
    public async Task Seeded_password_hashes_verify_the_demo_password_and_are_not_plaintext()
    {
        await using var context = _database.CreateContext();
        var passwords = new PasswordService();

        var hashes = await context.Users.AsNoTracking().Where(u => u.Id == SeedData.AliceId || u.Id == SeedData.BobId)
            .Select(u => u.PasswordHash).ToListAsync();

        Assert.Equal(2, hashes.Count);
        Assert.All(hashes, hash =>
        {
            Assert.NotEqual(SeedData.DemoPassword, hash);
            Assert.True(passwords.Verify(hash, SeedData.DemoPassword));
        });
    }

    [Fact]
    public async Task Seeded_tasks_cover_the_demo_scenarios_and_respect_domain_limits()
    {
        await using var context = _database.CreateContext();

        var alice = await context.TaskItems.AsNoTracking().Where(t => t.UserId == SeedData.AliceId).ToListAsync();
        var bob = await context.TaskItems.AsNoTracking().Where(t => t.UserId == SeedData.BobId).ToListAsync();

        // Scenarios 21–23: due exactly 2026-10-21T18:00Z (15:00 in São Paulo).
        Assert.Contains(alice, t => t.DueDateUtc == new DateTime(2026, 10, 21, 18, 0, 0, DateTimeKind.Utc));
        // Scenario 18: several overdue tasks in progress.
        Assert.True(alice.Count(t => t.Status == TaskItemStatus.InProgress && t.DueDateUtc < SeedReferenceDate) >= 2);
        // Scenario 19: a done task with a past due date.
        Assert.Contains(alice, t => t.Status == TaskItemStatus.Done && t.DueDateUtc < SeedReferenceDate);
        Assert.Contains(alice, t => t.DueDateUtc > SeedReferenceDate.AddMonths(1));
        Assert.Equal(3, bob.Count);

        Assert.All(alice.Concat(bob), t =>
        {
            Assert.InRange(t.Title.Length, 1, TaskItem.TitleMaxLength);
            Assert.True((t.Description?.Length ?? 0) <= TaskItem.DescriptionMaxLength);
            Assert.Equal(DateTimeKind.Utc, t.DueDateUtc.Kind);
        });
    }
}
