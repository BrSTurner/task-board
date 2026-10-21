using TaskBoard.Application.Abstractions;
using TaskBoard.Application.Auth;
using TaskBoard.Application.Common;
using TaskBoard.Domain.Entities;

namespace TaskBoard.Application.Tasks;

/// <summary>Creates a To Do task for the current user. Expects a request validated by <see cref="CreateTaskRequestValidator"/>.</summary>
public sealed class CreateTask(ITaskRepository tasks, IUserRepository users, ICurrentUser currentUser, TimeProvider time)
{
    public async Task<Result<TaskDto>> ExecuteAsync(CreateTaskRequest request, CancellationToken cancellationToken)
    {
        // A still-valid token whose user row is gone (e.g. the local database was reset) would otherwise
        // fail the foreign key with a 500.
        if (!await users.ExistsAsync(currentUser.UserId, cancellationToken))
        {
            return AuthErrors.SessionUserNotFound;
        }

        var timeZone = currentUser.GetTimeZone();
        var dueDateUtc = DueDates.ToUtc(request.DueDate!.Value, timeZone);
        if (!dueDateUtc.IsSuccess)
        {
            return dueDateUtc.Error!;
        }

        var task = TaskItem.Create(currentUser.UserId, request.Title, request.Description, dueDateUtc.Value);
        await tasks.AddAsync(task, cancellationToken);

        return TaskDto.From(task, timeZone, time.GetUtcNow().UtcDateTime);
    }
}
