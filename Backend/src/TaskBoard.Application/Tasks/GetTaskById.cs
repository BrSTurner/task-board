using TaskBoard.Application.Abstractions;
using TaskBoard.Application.Common;

namespace TaskBoard.Application.Tasks;

public sealed class GetTaskById(ITaskRepository tasks, ICurrentUser currentUser, TimeProvider time)
{
    public async Task<Result<TaskDto>> ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        var task = await tasks.GetForUserAsync(id, currentUser.UserId, cancellationToken);

        return task is null
            ? TaskErrors.NotFound
            : TaskDto.From(task, currentUser.GetTimeZone(), time.GetUtcNow().UtcDateTime);
    }
}
