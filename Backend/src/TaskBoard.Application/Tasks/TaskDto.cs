using TaskBoard.Domain.Entities;
using TaskBoard.Domain.Enums;

namespace TaskBoard.Application.Tasks;

/// <summary>Task representation sent to clients. Never exposes the owner id or other internals.</summary>
public sealed class TaskDto
{
    public required Guid Id { get; init; }

    public required string Title { get; init; }

    public required string? Description { get; init; }

    public required TaskItemStatus Status { get; init; }

    /// <summary>Due instant in UTC (serialized with a trailing <c>Z</c>).</summary>
    public required DateTime DueDateUtc { get; init; }

    /// <summary>The same instant as wall-clock time in the user's configured timezone (no offset).</summary>
    public required DateTime DueDateLocal { get; init; }

    /// <summary>BR-006: due before now (UTC) and not done. Computed when the response is produced.</summary>
    public required bool IsOverdue { get; init; }

    public static TaskDto From(TaskItem task, TimeZoneInfo timeZone, DateTime utcNow) => new()
    {
        Id = task.Id,
        Title = task.Title,
        Description = task.Description,
        Status = task.Status,
        DueDateUtc = task.DueDateUtc,
        DueDateLocal = DueDates.ToLocal(task.DueDateUtc, timeZone),
        IsOverdue = task.IsOverdue(utcNow),
    };
}
