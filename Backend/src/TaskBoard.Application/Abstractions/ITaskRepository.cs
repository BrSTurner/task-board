using TaskBoard.Domain.Entities;

namespace TaskBoard.Application.Abstractions;

public interface ITaskRepository
{
    Task<IReadOnlyList<TaskItem>> ListByUserAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Returns the task only when it belongs to <paramref name="userId"/>. Absent and not-owned tasks are
    /// indistinguishable (both <c>null</c>) so callers return 404 without enabling id enumeration.
    /// </summary>
    Task<TaskItem?> GetForUserAsync(Guid id, Guid userId, CancellationToken cancellationToken);

    Task AddAsync(TaskItem task, CancellationToken cancellationToken);

    Task UpdateAsync(TaskItem task, CancellationToken cancellationToken);

    Task DeleteAsync(TaskItem task, CancellationToken cancellationToken);
}
