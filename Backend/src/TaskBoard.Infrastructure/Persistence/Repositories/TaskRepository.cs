using Microsoft.EntityFrameworkCore;
using TaskBoard.Application.Abstractions;
using TaskBoard.Domain.Entities;

namespace TaskBoard.Infrastructure.Persistence.Repositories;

internal sealed class TaskRepository(TaskBoardDbContext db) : ITaskRepository
{
    public async Task<IReadOnlyList<TaskItem>> ListByUserAsync(Guid userId, CancellationToken cancellationToken) =>
        // Final board ordering (overdue first) depends on "now" and is applied by the application layer.
        await db.TaskItems.AsNoTracking()
            .Where(t => t.UserId == userId)
            .OrderBy(t => t.DueDateUtc)
            .ToListAsync(cancellationToken);

    public Task<TaskItem?> GetForUserAsync(Guid id, Guid userId, CancellationToken cancellationToken) =>
        // Ownership is part of the query, so another user's task is simply "not found".
        db.TaskItems.SingleOrDefaultAsync(t => t.Id == id && t.UserId == userId, cancellationToken);

    public async Task AddAsync(TaskItem task, CancellationToken cancellationToken)
    {
        db.TaskItems.Add(task);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(TaskItem task, CancellationToken cancellationToken)
    {
        db.TaskItems.Update(task);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(TaskItem task, CancellationToken cancellationToken)
    {
        db.TaskItems.Remove(task);
        await db.SaveChangesAsync(cancellationToken);
    }
}
