using TaskBoard.Domain.Entities;
using TaskBoard.Domain.Enums;
using TaskBoard.Domain.Exceptions;

namespace TaskBoard.Domain.Tests.Entities;

public sealed class TaskItemTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime DueUtc = new(2026, 10, 21, 18, 0, 0, DateTimeKind.Utc);

    private static TaskItem NewTask(string title = "Prepare technical interview", string? description = "Review Clean Architecture") =>
        TaskItem.Create(UserId, title, description, DueUtc);

    [Fact]
    public void Create_defaults_to_todo_and_assigns_owner_and_id()
    {
        var task = NewTask();

        Assert.NotEqual(Guid.Empty, task.Id);
        Assert.Equal(UserId, task.UserId);
        Assert.Equal(TaskItemStatus.Todo, task.Status);
        Assert.Equal("Prepare technical interview", task.Title);
        Assert.Equal("Review Clean Architecture", task.Description);
        Assert.Equal(DueUtc, task.DueDateUtc);
    }

    [Fact]
    public void Create_trims_title_and_description()
    {
        var task = NewTask("  Padded title  ", "  Padded description  ");

        Assert.Equal("Padded title", task.Title);
        Assert.Equal("Padded description", task.Description);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_stores_blank_description_as_null(string? description)
    {
        Assert.Null(NewTask(description: description).Description);
    }

    [Fact]
    public void Title_limit_is_measured_after_trimming()
    {
        var title = "  " + new string('t', TaskItem.TitleMaxLength) + "  ";

        Assert.Equal(TaskItem.TitleMaxLength, NewTask(title).Title.Length);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_rejects_blank_title(string title)
    {
        Assert.Throws<DomainException>(() => NewTask(title));
    }

    [Fact]
    public void Create_rejects_title_longer_than_50_characters()
    {
        Assert.Throws<DomainException>(() => NewTask(new string('t', TaskItem.TitleMaxLength + 1)));
    }

    [Fact]
    public void Create_rejects_description_longer_than_400_characters()
    {
        Assert.Throws<DomainException>(() => NewTask(description: new string('d', TaskItem.DescriptionMaxLength + 1)));
    }

    [Fact]
    public void Create_rejects_missing_owner()
    {
        Assert.Throws<DomainException>(() => TaskItem.Create(Guid.Empty, "Title", null, DueUtc));
    }

    [Theory]
    [InlineData(DateTimeKind.Local)]
    [InlineData(DateTimeKind.Unspecified)]
    public void Create_rejects_due_dates_that_are_not_utc(DateTimeKind kind)
    {
        var due = DateTime.SpecifyKind(DueUtc, kind);

        Assert.Throws<DomainException>(() => TaskItem.Create(UserId, "Title", null, due));
    }

    [Fact]
    public void UpdateDetails_replaces_editable_fields()
    {
        var task = NewTask();
        var newDue = DueUtc.AddDays(3);

        task.UpdateDetails(" New title ", "", newDue, TaskItemStatus.InProgress);

        Assert.Equal("New title", task.Title);
        Assert.Null(task.Description);
        Assert.Equal(newDue, task.DueDateUtc);
        Assert.Equal(TaskItemStatus.InProgress, task.Status);
    }

    [Fact]
    public void UpdateDetails_guards_invariants()
    {
        var task = NewTask();

        Assert.Throws<DomainException>(() => task.UpdateDetails(" ", null, DueUtc, TaskItemStatus.Todo));
        Assert.Equal("Prepare technical interview", task.Title);
    }

    [Theory]
    [InlineData(TaskItemStatus.InProgress)]
    [InlineData(TaskItemStatus.Done)]
    [InlineData(TaskItemStatus.Todo)]
    public void ChangeStatus_allows_any_column_including_moving_backwards(TaskItemStatus status)
    {
        var task = NewTask();
        task.ChangeStatus(TaskItemStatus.Done);

        task.ChangeStatus(status);

        Assert.Equal(status, task.Status);
    }

    [Fact]
    public void ChangeStatus_rejects_undefined_values()
    {
        Assert.Throws<DomainException>(() => NewTask().ChangeStatus((TaskItemStatus)42));
    }

    [Fact]
    public void Task_is_overdue_when_due_before_now_and_not_done()
    {
        // Scenario 23: due 18:00Z, now 18:01Z.
        var task = NewTask();

        Assert.True(task.IsOverdue(DueUtc.AddMinutes(1)));
    }

    [Fact]
    public void Task_due_exactly_now_or_later_is_not_overdue()
    {
        var task = NewTask();

        Assert.False(task.IsOverdue(DueUtc));
        Assert.False(task.IsOverdue(DueUtc.AddMinutes(-1)));
    }

    [Fact]
    public void Done_task_is_never_overdue()
    {
        // Scenarios 19 and 20.
        var task = NewTask();
        task.ChangeStatus(TaskItemStatus.Done);

        Assert.False(task.IsOverdue(DueUtc.AddDays(30)));
    }
}
