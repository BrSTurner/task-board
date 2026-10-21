using TaskBoard.Application.Abstractions;
using TaskBoard.Domain.Entities;

namespace TaskBoard.Application.Tests.Fakes;

internal sealed class InMemoryTaskRepository : ITaskRepository
{
    public List<TaskItem> Tasks { get; } = [];

    public Task<IReadOnlyList<TaskItem>> ListByUserAsync(Guid userId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<TaskItem>>(Tasks.Where(t => t.UserId == userId).ToList());

    public Task<TaskItem?> GetForUserAsync(Guid id, Guid userId, CancellationToken cancellationToken) =>
        Task.FromResult(Tasks.SingleOrDefault(t => t.Id == id && t.UserId == userId));

    public Task AddAsync(TaskItem task, CancellationToken cancellationToken)
    {
        Tasks.Add(task);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(TaskItem task, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task DeleteAsync(TaskItem task, CancellationToken cancellationToken)
    {
        Tasks.Remove(task);
        return Task.CompletedTask;
    }
}
