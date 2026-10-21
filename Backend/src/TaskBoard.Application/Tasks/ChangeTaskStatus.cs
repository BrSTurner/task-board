using TaskBoard.Application.Abstractions;
using TaskBoard.Application.Common;

namespace TaskBoard.Application.Tasks;

/// <summary>Moves an owned task to another Kanban column (drag-and-drop).</summary>
public sealed class ChangeTaskStatus(ITaskRepository tasks, ICurrentUser currentUser, TimeProvider time)
{
    public async Task<Result<TaskDto>> ExecuteAsync(Guid id, ChangeTaskStatusRequest request, CancellationToken cancellationToken)
    {
        var task = await tasks.GetForUserAsync(id, currentUser.UserId, cancellationToken);
        if (task is null)
        {
            return TaskErrors.NotFound;
        }

        task.ChangeStatus(request.Status!.Value);
        await tasks.UpdateAsync(task, cancellationToken);

        // The response carries the recomputed IsOverdue (e.g. false once moved to Done).
        return TaskDto.From(task, currentUser.GetTimeZone(), time.GetUtcNow().UtcDateTime);
    }
}
