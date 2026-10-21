namespace TaskBoard.Application.Tasks;

public static class TaskOrdering
{
    /// <summary>
    /// BR-007: overdue tasks first (earliest due first), then by due date, title and id for a deterministic
    /// order. There is no persisted manual ordering within a column. The Angular board applies the same rule.
    /// </summary>
    public static IReadOnlyList<TaskDto> Order(IEnumerable<TaskDto> tasks) =>
        tasks.OrderByDescending(t => t.IsOverdue)
            .ThenBy(t => t.DueDateUtc)
            .ThenBy(t => t.Title, StringComparer.OrdinalIgnoreCase)
            .ThenBy(t => t.Id)
            .ToList();
}
