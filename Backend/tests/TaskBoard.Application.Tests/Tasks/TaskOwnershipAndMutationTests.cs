using TaskBoard.Application.Common;
using TaskBoard.Application.Tasks;
using TaskBoard.Domain.Enums;

namespace TaskBoard.Application.Tests.Tasks;

public sealed class TaskOwnershipAndMutationTests
{
    private readonly TaskTestContext _ctx = new();
    private static readonly DateTime Now = TaskTestContext.Now;

    [Fact]
    public async Task Get_by_id_returns_own_task()
    {
        var task = _ctx.AddTask(_ctx.Alice, "Mine", Now.AddDays(1));

        var result = await new GetTaskById(_ctx.Tasks, _ctx.As(_ctx.Alice), _ctx.Time).ExecuteAsync(task.Id, CancellationToken.None);

        Assert.Equal("Mine", result.Value.Title);
    }

    [Fact]
    public async Task Other_users_task_is_not_found_for_every_operation_and_stays_unchanged()
    {
        // Scenario 24.
        var bobsTask = _ctx.AddTask(_ctx.Bob, "Bob's secret", Now.AddDays(1));
        var asAlice = _ctx.As(_ctx.Alice);

        var get = await new GetTaskById(_ctx.Tasks, asAlice, _ctx.Time).ExecuteAsync(bobsTask.Id, CancellationToken.None);
        var update = await new UpdateTask(_ctx.Tasks, asAlice, _ctx.Time).ExecuteAsync(bobsTask.Id,
            new UpdateTaskRequest { Title = "Hacked", DueDate = new DateTime(2026, 12, 1, 9, 0, 0), Status = TaskItemStatus.Done }, CancellationToken.None);
        var move = await new ChangeTaskStatus(_ctx.Tasks, asAlice, _ctx.Time).ExecuteAsync(bobsTask.Id,
            new ChangeTaskStatusRequest { Status = TaskItemStatus.Done }, CancellationToken.None);
        var delete = await new DeleteTask(_ctx.Tasks, asAlice).ExecuteAsync(bobsTask.Id, CancellationToken.None);

        Assert.All([get.Error, update.Error, move.Error, delete.Error], error => Assert.Equal(TaskErrors.NotFound, error));
        Assert.Contains(bobsTask, _ctx.Tasks.Tasks);
        Assert.Equal("Bob's secret", bobsTask.Title);
        Assert.Equal(TaskItemStatus.Todo, bobsTask.Status);
    }

    [Fact]
    public async Task Unknown_task_id_is_not_found()
    {
        var result = await new GetTaskById(_ctx.Tasks, _ctx.As(_ctx.Alice), _ctx.Time).ExecuteAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public async Task Update_replaces_fields_and_converts_due_date()
    {
        // Scenario 13.
        var task = _ctx.AddTask(_ctx.Alice, "Old", Now.AddDays(1));

        var result = await new UpdateTask(_ctx.Tasks, _ctx.As(_ctx.Alice), _ctx.Time).ExecuteAsync(task.Id, new UpdateTaskRequest
        {
            Title = "New",
            Description = "Details",
            DueDate = new DateTime(2026, 11, 5, 10, 0, 0),
            Status = TaskItemStatus.InProgress,
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("New", task.Title);
        Assert.Equal("Details", task.Description);
        Assert.Equal(TaskItemStatus.InProgress, task.Status);
        Assert.Equal(new DateTime(2026, 11, 5, 13, 0, 0, DateTimeKind.Utc), task.DueDateUtc);
        Assert.Equal("New", result.Value.Title);
    }

    [Fact]
    public async Task Update_with_unchanged_local_due_date_keeps_the_stored_instant()
    {
        // 01:30 on 2026-11-01 is ambiguous in New York; re-converting would always pick standard time.
        var dueUtc = new DateTime(2026, 11, 1, 5, 30, 0, DateTimeKind.Utc); // 01:30 EDT (first occurrence)
        var task = _ctx.AddTask(_ctx.Alice, "Ambiguous", dueUtc);
        var asNewYorker = new Fakes.FakeCurrentUser(_ctx.Alice.Id, "America/New_York");

        await new UpdateTask(_ctx.Tasks, asNewYorker, _ctx.Time).ExecuteAsync(task.Id, new UpdateTaskRequest
        {
            Title = "Renamed only",
            DueDate = new DateTime(2026, 11, 1, 1, 30, 0),
            Status = TaskItemStatus.Todo,
        }, CancellationToken.None);

        Assert.Equal(dueUtc, task.DueDateUtc);
    }

    [Fact]
    public async Task Moving_an_overdue_task_to_done_clears_the_overdue_flag()
    {
        // Scenarios 14 and 20.
        var task = _ctx.AddTask(_ctx.Alice, "Late", Now.AddDays(-1), TaskItemStatus.InProgress);

        var result = await new ChangeTaskStatus(_ctx.Tasks, _ctx.As(_ctx.Alice), _ctx.Time)
            .ExecuteAsync(task.Id, new ChangeTaskStatusRequest { Status = TaskItemStatus.Done }, CancellationToken.None);

        Assert.Equal(TaskItemStatus.Done, task.Status);
        Assert.Equal(TaskItemStatus.Done, result.Value.Status);
        Assert.False(result.Value.IsOverdue);
    }

    [Fact]
    public async Task Delete_removes_own_task()
    {
        // Scenario 16.
        var task = _ctx.AddTask(_ctx.Alice, "Disposable", Now.AddDays(1));

        var result = await new DeleteTask(_ctx.Tasks, _ctx.As(_ctx.Alice)).ExecuteAsync(task.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.DoesNotContain(task, _ctx.Tasks.Tasks);
    }
}
