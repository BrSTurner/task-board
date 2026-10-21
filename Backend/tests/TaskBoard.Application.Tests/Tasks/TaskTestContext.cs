using Microsoft.Extensions.Time.Testing;
using TaskBoard.Application.Tests.Fakes;
using TaskBoard.Domain.Entities;
using TaskBoard.Domain.Enums;

namespace TaskBoard.Application.Tests.Tasks;

/// <summary>Shared arrangement: Alice (São Paulo, UTC-3) and Bob (Tokyo, UTC+9), clock fixed at 2026-10-21 12:00Z.</summary>
internal sealed class TaskTestContext
{
    public static readonly DateTime Now = new(2026, 10, 21, 12, 0, 0, DateTimeKind.Utc);

    public TaskTestContext()
    {
        Users.Users.Add(Alice);
        Users.Users.Add(Bob);
    }

    public User Alice { get; } = User.Create("Alice", "alice@example.test", "America/Sao_Paulo", "hash");

    public User Bob { get; } = User.Create("Bob", "bob@example.test", "Asia/Tokyo", "hash");

    public InMemoryUserRepository Users { get; } = new();

    public InMemoryTaskRepository Tasks { get; } = new();

    public FakeTimeProvider Time { get; } = new(new DateTimeOffset(Now));

    public FakeCurrentUser As(User user) => new(user.Id, user.TimeZone);

    public TaskItem AddTask(User owner, string title, DateTime dueUtc, TaskItemStatus status = TaskItemStatus.Todo)
    {
        var task = TaskItem.Create(owner.Id, title, null, dueUtc);
        task.ChangeStatus(status);
        Tasks.Tasks.Add(task);
        return task;
    }
}
