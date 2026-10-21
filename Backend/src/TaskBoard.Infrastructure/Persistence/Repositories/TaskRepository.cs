using TaskBoard.Application.Abstractions;
using TaskBoard.Domain.Entities;

namespace TaskBoard.Infrastructure.Persistence.Repositories;

internal sealed class TaskRepository(TaskBoardDbContext db) : ITaskRepository
{
    // TDD stub — implemented in the GREEN step.
    public Task<IReadOnlyList<TaskItem>> ListByUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        _ = db;
        return Task.FromResult<IReadOnlyList<TaskItem>>([]);
    }

    public Task<TaskItem?> GetForUserAsync(Guid id, Guid userId, CancellationToken cancellationToken) => Task.FromResult<TaskItem?>(null);

    public Task AddAsync(TaskItem task, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task UpdateAsync(TaskItem task, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task DeleteAsync(TaskItem task, CancellationToken cancellationToken) => Task.CompletedTask;
}
