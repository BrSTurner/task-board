using TaskBoard.Domain.Enums;

namespace TaskBoard.Domain.Entities;

public sealed class TaskItem
{
    public const int TitleMaxLength = 50;
    public const int DescriptionMaxLength = 400;

    private TaskItem()
    {
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public TaskItemStatus Status { get; private set; }

    public DateTime DueDateUtc { get; private set; }

    // TDD stubs — implemented in the GREEN step.
    public static TaskItem Create(Guid userId, string title, string? description, DateTime dueDateUtc) => new();

    public void UpdateDetails(string title, string? description, DateTime dueDateUtc, TaskItemStatus status)
    {
    }

    public void ChangeStatus(TaskItemStatus status)
    {
    }

    public bool IsOverdue(DateTime utcNow) => false;
}
