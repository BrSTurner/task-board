using TaskBoard.Application.Common;
using TaskBoard.Application.Tasks;
using TaskBoard.Application.Tests.Fakes;
using TaskBoard.Domain.Enums;

namespace TaskBoard.Application.Tests.Tasks;

public sealed class CreateTaskTests
{
    private readonly TaskTestContext _ctx = new();

    private CreateTask CreateFor(Guid userId, string timeZone = "America/Sao_Paulo") =>
        new(_ctx.Tasks, _ctx.Users, new FakeCurrentUser(userId, timeZone), _ctx.Time);

    [Fact]
    public async Task Creates_todo_task_owned_by_the_current_user_with_utc_due_date()
    {
        // Scenarios 8 and 21.
        var request = new CreateTaskRequest
        {
            Title = "Prepare technical interview",
            Description = "Review Clean Architecture",
            DueDate = new DateTime(2026, 10, 21, 15, 0, 0),
        };

        var result = await CreateFor(_ctx.Alice.Id).ExecuteAsync(request, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var stored = Assert.Single(_ctx.Tasks.Tasks);
        Assert.Equal(_ctx.Alice.Id, stored.UserId);
        Assert.Equal(TaskItemStatus.Todo, stored.Status);
        Assert.Equal(new DateTime(2026, 10, 21, 18, 0, 0, DateTimeKind.Utc), stored.DueDateUtc);
        Assert.Equal(stored.Id, result.Value.Id);
        Assert.Equal(TaskItemStatus.Todo, result.Value.Status);
        Assert.Equal(new DateTime(2026, 10, 21, 15, 0, 0), result.Value.DueDateLocal);
        Assert.False(result.Value.IsOverdue);
    }

    [Fact]
    public async Task Description_is_optional()
    {
        // Scenario 9.
        var result = await CreateFor(_ctx.Alice.Id).ExecuteAsync(
            new CreateTaskRequest { Title = "No description", DueDate = new DateTime(2026, 12, 1, 9, 0, 0) }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.Description);
    }

    [Fact]
    public async Task Past_due_date_is_allowed_and_reported_as_overdue()
    {
        var result = await CreateFor(_ctx.Alice.Id).ExecuteAsync(
            new CreateTaskRequest { Title = "Already late", DueDate = new DateTime(2026, 10, 1, 9, 0, 0) }, CancellationToken.None);

        Assert.True(result.Value.IsOverdue);
    }

    [Fact]
    public async Task Due_date_inside_a_dst_gap_is_rejected_and_nothing_is_persisted()
    {
        var result = await CreateFor(_ctx.Alice.Id, "America/New_York").ExecuteAsync(
            new CreateTaskRequest { Title = "Gap", DueDate = new DateTime(2026, 3, 8, 2, 30, 0) }, CancellationToken.None);

        Assert.Equal(ErrorType.Validation, result.Error!.Type);
        Assert.Empty(_ctx.Tasks.Tasks);
    }

    [Fact]
    public async Task Token_for_a_deleted_user_is_unauthorized_instead_of_violating_the_foreign_key()
    {
        var result = await CreateFor(Guid.NewGuid()).ExecuteAsync(
            new CreateTaskRequest { Title = "Orphan", DueDate = new DateTime(2026, 12, 1, 9, 0, 0) }, CancellationToken.None);

        Assert.Equal(ErrorType.Unauthorized, result.Error!.Type);
        Assert.Empty(_ctx.Tasks.Tasks);
    }
}
