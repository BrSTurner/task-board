using TaskBoard.Application.Tasks;
using TaskBoard.Domain.Enums;

namespace TaskBoard.Application.Tests.Tasks;

public sealed class GetMyTasksTests
{
    private readonly TaskTestContext _ctx = new();
    private static readonly DateTime Now = TaskTestContext.Now;

    private async Task<IReadOnlyList<TaskDto>> ListFor(Domain.Entities.User user)
    {
        var result = await new GetMyTasks(_ctx.Tasks, _ctx.As(user), _ctx.Time).ExecuteAsync(CancellationToken.None);
        Assert.True(result.IsSuccess);
        return result.Value;
    }

    [Fact]
    public async Task Returns_only_the_current_users_tasks()
    {
        _ctx.AddTask(_ctx.Alice, "Alice task", Now.AddDays(1));
        _ctx.AddTask(_ctx.Bob, "Bob task", Now.AddDays(1));

        var tasks = await ListFor(_ctx.Alice);

        Assert.Equal(["Alice task"], tasks.Select(t => t.Title));
    }

    [Fact]
    public async Task Empty_board_returns_an_empty_list()
    {
        Assert.Empty(await ListFor(_ctx.Alice));
    }

    [Fact]
    public async Task Overdue_tasks_come_first_ordered_by_earliest_due_date()
    {
        // Scenarios 17 and 18.
        _ctx.AddTask(_ctx.Alice, "Future B", Now.AddDays(2), TaskItemStatus.InProgress);
        _ctx.AddTask(_ctx.Alice, "Overdue late", Now.AddHours(-1), TaskItemStatus.InProgress);
        _ctx.AddTask(_ctx.Alice, "Future A", Now.AddDays(1), TaskItemStatus.InProgress);
        _ctx.AddTask(_ctx.Alice, "Overdue early", Now.AddDays(-3), TaskItemStatus.InProgress);

        var tasks = await ListFor(_ctx.Alice);

        Assert.Equal(["Overdue early", "Overdue late", "Future A", "Future B"], tasks.Select(t => t.Title));
        Assert.Equal([true, true, false, false], tasks.Select(t => t.IsOverdue));
    }

    [Fact]
    public async Task Done_task_with_past_due_date_is_not_overdue()
    {
        // Scenario 19.
        _ctx.AddTask(_ctx.Alice, "Finished", Now.AddDays(-5), TaskItemStatus.Done);

        var task = Assert.Single(await ListFor(_ctx.Alice));

        Assert.False(task.IsOverdue);
        Assert.Equal(TaskItemStatus.Done, task.Status);
    }

    [Fact]
    public async Task Due_date_is_shown_in_the_users_timezone()
    {
        // Scenario 22.
        _ctx.AddTask(_ctx.Alice, "Interview", new DateTime(2026, 10, 21, 18, 0, 0, DateTimeKind.Utc));

        var task = Assert.Single(await ListFor(_ctx.Alice));

        Assert.Equal(new DateTime(2026, 10, 21, 15, 0, 0), task.DueDateLocal);
        Assert.Equal(new DateTime(2026, 10, 21, 18, 0, 0, DateTimeKind.Utc), task.DueDateUtc);
    }

    [Fact]
    public async Task Overdue_detection_does_not_depend_on_the_viewers_timezone()
    {
        // Scenario 23: due 18:00Z, now 18:01Z → overdue for São Paulo and Tokyo alike.
        _ctx.Time.SetUtcNow(new DateTimeOffset(2026, 10, 21, 18, 1, 0, TimeSpan.Zero));
        var due = new DateTime(2026, 10, 21, 18, 0, 0, DateTimeKind.Utc);
        _ctx.AddTask(_ctx.Alice, "Sao Paulo view", due);
        _ctx.AddTask(_ctx.Bob, "Tokyo view", due);

        var alice = Assert.Single(await ListFor(_ctx.Alice));
        var bob = Assert.Single(await ListFor(_ctx.Bob));

        Assert.True(alice.IsOverdue);
        Assert.True(bob.IsOverdue);
        Assert.Equal(new DateTime(2026, 10, 22, 3, 0, 0), bob.DueDateLocal);
    }
}
