using TaskBoard.Application.Abstractions;
using TaskBoard.Application.Common;

namespace TaskBoard.Application.Tasks;

public sealed class GetMyTasks(ITaskRepository tasks, ICurrentUser currentUser, TimeProvider time)
{
    public async Task<Result<IReadOnlyList<TaskDto>>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var timeZone = currentUser.GetTimeZone();
        var now = time.GetUtcNow().UtcDateTime;

        var items = await tasks.ListByUserAsync(currentUser.UserId, cancellationToken);

        return Result<IReadOnlyList<TaskDto>>.Success(TaskOrdering.Order(items.Select(t => TaskDto.From(t, timeZone, now))));
    }
}
