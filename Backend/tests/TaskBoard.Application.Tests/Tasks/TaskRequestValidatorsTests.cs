using TaskBoard.Application.Tasks;
using TaskBoard.Domain.Enums;

namespace TaskBoard.Application.Tests.Tasks;

public sealed class TaskRequestValidatorsTests
{
    private static readonly DateTime LocalDue = new(2026, 10, 21, 15, 0, 0, DateTimeKind.Unspecified);

    private readonly CreateTaskRequestValidator _create = new();
    private readonly UpdateTaskRequestValidator _update = new();
    private readonly ChangeTaskStatusRequestValidator _changeStatus = new();

    private static string? MessageFor(FluentValidation.Results.ValidationResult result, string property) =>
        result.Errors.SingleOrDefault(e => e.PropertyName == property)?.ErrorMessage;

    [Fact]
    public void Valid_create_request_passes()
    {
        Assert.True(_create.Validate(new CreateTaskRequest { Title = "Prepare interview", DueDate = LocalDue }).IsValid);
    }

    [Fact]
    public void Create_requires_title_and_due_date_with_exact_messages()
    {
        // Scenario 10.
        var result = _create.Validate(new CreateTaskRequest { Title = "  ", DueDate = null });

        Assert.Equal("Title is required", MessageFor(result, nameof(CreateTaskRequest.Title)));
        Assert.Equal("Due date is required", MessageFor(result, nameof(CreateTaskRequest.DueDate)));
    }

    [Fact]
    public void Create_enforces_title_and_description_limits()
    {
        // Scenario 11.
        var result = _create.Validate(new CreateTaskRequest
        {
            Title = new string('t', 51),
            Description = new string('d', 401),
            DueDate = LocalDue,
        });

        Assert.Equal("Title must be at most 50 characters", MessageFor(result, nameof(CreateTaskRequest.Title)));
        Assert.Equal("Description must be at most 400 characters", MessageFor(result, nameof(CreateTaskRequest.Description)));
    }

    [Fact]
    public void Limits_are_measured_after_trimming()
    {
        var request = new CreateTaskRequest { Title = "  " + new string('t', 50) + "  ", Description = new string('d', 400) + "   ", DueDate = LocalDue };

        Assert.True(_create.Validate(request).IsValid);
    }

    [Theory]
    [InlineData(DateTimeKind.Utc)]
    [InlineData(DateTimeKind.Local)]
    public void Due_date_with_an_offset_is_rejected(DateTimeKind kind)
    {
        var result = _create.Validate(new CreateTaskRequest { Title = "T", DueDate = DateTime.SpecifyKind(LocalDue, kind) });

        Assert.NotNull(MessageFor(result, nameof(CreateTaskRequest.DueDate)));
    }

    [Fact]
    public void Update_requires_a_valid_status()
    {
        var missing = _update.Validate(new UpdateTaskRequest { Title = "T", DueDate = LocalDue, Status = null });
        var undefined = _update.Validate(new UpdateTaskRequest { Title = "T", DueDate = LocalDue, Status = (TaskItemStatus)42 });

        Assert.Equal("Status is required", MessageFor(missing, nameof(UpdateTaskRequest.Status)));
        Assert.NotNull(MessageFor(undefined, nameof(UpdateTaskRequest.Status)));
    }

    [Fact]
    public void Update_applies_the_same_field_rules_as_create()
    {
        var result = _update.Validate(new UpdateTaskRequest { Title = "", DueDate = null, Status = TaskItemStatus.Done });

        Assert.Equal("Title is required", MessageFor(result, nameof(UpdateTaskRequest.Title)));
        Assert.Equal("Due date is required", MessageFor(result, nameof(UpdateTaskRequest.DueDate)));
    }

    [Fact]
    public void Change_status_requires_a_defined_status()
    {
        Assert.False(_changeStatus.Validate(new ChangeTaskStatusRequest { Status = null }).IsValid);
        Assert.False(_changeStatus.Validate(new ChangeTaskStatusRequest { Status = (TaskItemStatus)7 }).IsValid);
        Assert.True(_changeStatus.Validate(new ChangeTaskStatusRequest { Status = TaskItemStatus.InProgress }).IsValid);
    }
}
