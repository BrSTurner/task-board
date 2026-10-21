using TaskBoard.Domain.Enums;
using TaskBoard.Domain.Exceptions;

namespace TaskBoard.Domain.Entities;

public sealed class TaskItem
{
    public const int TitleMaxLength = 50;
    public const int DescriptionMaxLength = 400;

    private TaskItem()
    {
    }

    public Guid Id { get; private set; }

    /// <summary>Owner. Always taken from the authenticated user, never from the client.</summary>
    public Guid UserId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public TaskItemStatus Status { get; private set; }

    /// <summary>Due instant in UTC. Conversion from the user's local time happens in the application layer.</summary>
    public DateTime DueDateUtc { get; private set; }

    public static TaskItem Create(Guid userId, string title, string? description, DateTime dueDateUtc)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("A task must belong to a user.");
        }

        var task = new TaskItem
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            Status = TaskItemStatus.Todo,
        };
        task.SetDetails(title, description, dueDateUtc);
        return task;
    }

    public void UpdateDetails(string title, string? description, DateTime dueDateUtc, TaskItemStatus status)
    {
        EnsureDefined(status);
        SetDetails(title, description, dueDateUtc);
        Status = status;
    }

    /// <summary>Moves the task to any Kanban column, including backwards; there is no approval workflow.</summary>
    public void ChangeStatus(TaskItemStatus status)
    {
        EnsureDefined(status);
        Status = status;
    }

    /// <summary>BR-006: overdue when the due instant is earlier than now (UTC) and the task is not done.</summary>
    public bool IsOverdue(DateTime utcNow) => Status != TaskItemStatus.Done && DueDateUtc < utcNow;

    // Validates everything before assigning anything, so a failed update leaves the task unchanged.
    private void SetDetails(string title, string? description, DateTime dueDateUtc)
    {
        var validTitle = Guard.RequiredText(title, nameof(Title), TitleMaxLength);
        var trimmedDescription = description?.Trim();
        var validDescription = string.IsNullOrEmpty(trimmedDescription)
            ? null
            : Guard.MaxLength(trimmedDescription, nameof(Description), DescriptionMaxLength);
        if (dueDateUtc.Kind != DateTimeKind.Utc)
        {
            throw new DomainException("Due date must be expressed in UTC.");
        }

        Title = validTitle;
        Description = validDescription;
        DueDateUtc = dueDateUtc;
    }

    private static void EnsureDefined(TaskItemStatus status)
    {
        if (!Enum.IsDefined(status))
        {
            throw new DomainException($"'{status}' is not a valid task status.");
        }
    }
}
