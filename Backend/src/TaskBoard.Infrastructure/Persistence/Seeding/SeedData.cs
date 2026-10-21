using TaskBoard.Domain.Enums;

namespace TaskBoard.Infrastructure.Persistence.Seeding;

/// <summary>
/// Demo data applied through EF Core <c>HasData</c>, i.e. as part of the migrations (Code First seeding).
/// Every value must be a constant: anything computed at runtime (<c>Guid.NewGuid()</c>, <c>DateTime.UtcNow</c>,
/// a freshly salted hash) would change the model on every build and break <c>Migrate()</c>.
/// </summary>
/// <remarks>
/// Because the rows live in a migration they are inserted exactly once per database, in every environment where
/// migrations are applied. They are demo accounts — do not apply these migrations to a production database
/// without removing them. Dates are fixed around 2026-10-21 (the exercise date), so some tasks are overdue.
/// </remarks>
public static class SeedData
{
    public const string DemoPassword = "DemoPass123!";

    public static readonly Guid AliceId = Guid.Parse("a11ce000-0000-7000-8000-000000000001");
    public static readonly Guid BobId = Guid.Parse("b0b00000-0000-7000-8000-000000000002");

    // PasswordHasher (Identity V3, PBKDF2-SHA512, 100k iterations) hash of DemoPassword, generated once.
    private const string DemoPasswordHash = "AQAAAAIAAYagAAAAECuoL6m7HNzz0v7D5ihyiEnloVLgSf0Q11bu2OECMqfsdKZybfgw4KUYjkAGF2DEpw==";

    // Anonymous objects bypass the entities' private constructors; values are kept valid by SeedDataTests.
    internal static object[] Users =>
    [
        new { Id = AliceId, Name = "Alice Martins", Email = "demo@example.test", TimeZone = "America/Sao_Paulo", PasswordHash = DemoPasswordHash },
        new { Id = BobId, Name = "Bob Tanaka", Email = "bob@example.test", TimeZone = "Asia/Tokyo", PasswordHash = DemoPasswordHash },
    ];

    internal static object[] Tasks =>
    [
        // Alice (America/Sao_Paulo, UTC-3)
        Task("a11ce000-7a5c-7000-8000-000000000001", AliceId, "Prepare technical interview", "Review Clean Architecture", TaskItemStatus.Todo, Utc(2026, 10, 21, 18, 0)),
        Task("a11ce000-7a5c-7000-8000-000000000002", AliceId, "Fix flaky login test", "Intermittent timeout on CI.", TaskItemStatus.InProgress, Utc(2026, 10, 14, 13, 0)),
        Task("a11ce000-7a5c-7000-8000-000000000003", AliceId, "Write API documentation", "Describe the auth and tasks endpoints.", TaskItemStatus.InProgress, Utc(2026, 10, 18, 20, 0)),
        Task("a11ce000-7a5c-7000-8000-000000000004", AliceId, "Renew passport", null, TaskItemStatus.Todo, Utc(2026, 10, 10, 12, 0)),
        Task("a11ce000-7a5c-7000-8000-000000000005", AliceId, "Submit expense report", "September travel expenses.", TaskItemStatus.Done, Utc(2026, 10, 5, 21, 0)),
        Task("a11ce000-7a5c-7000-8000-000000000006", AliceId, "Refactor task board styles", null, TaskItemStatus.InProgress, Utc(2026, 11, 20, 17, 0)),
        Task("a11ce000-7a5c-7000-8000-000000000007", AliceId, "Book dentist appointment", null, TaskItemStatus.Todo, Utc(2026, 12, 3, 12, 30)),
        Task("a11ce000-7a5c-7000-8000-000000000008", AliceId, "Plan Q1 roadmap", "Draft goals and milestones for next quarter.", TaskItemStatus.Todo, Utc(2027, 1, 15, 13, 0)),

        // Bob (Asia/Tokyo, UTC+9)
        Task("b0b00000-7a5c-7000-8000-000000000001", BobId, "Review pull requests", null, TaskItemStatus.InProgress, Utc(2026, 10, 20, 1, 0)),
        Task("b0b00000-7a5c-7000-8000-000000000002", BobId, "Prepare sprint demo", "Slides and a short live demo.", TaskItemStatus.Todo, Utc(2026, 11, 2, 0, 0)),
        Task("b0b00000-7a5c-7000-8000-000000000003", BobId, "Update team wiki", null, TaskItemStatus.Done, Utc(2026, 10, 15, 8, 0)),
    ];

    private static object Task(string id, Guid userId, string title, string? description, TaskItemStatus status, DateTime dueDateUtc) =>
        new { Id = Guid.Parse(id), UserId = userId, Title = title, Description = description, Status = status, DueDateUtc = dueDateUtc };

    private static DateTime Utc(int year, int month, int day, int hour, int minute) =>
        new(year, month, day, hour, minute, 0, DateTimeKind.Utc);
}
