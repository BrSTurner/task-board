using TaskBoard.Application.Abstractions;
using TaskBoard.Application.Common;

namespace TaskBoard.Application.Tasks;

public sealed class DeleteTask(ITaskRepository tasks, ICurrentUser currentUser)
{
    public async Task<Result> ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        var task = await tasks.GetForUserAsync(id, currentUser.UserId, cancellationToken);
        if (task is null)
        {
            return TaskErrors.NotFound;
        }

        await tasks.DeleteAsync(task, cancellationToken);
        return Result.Success();
    }
}
