using TaskBoard.Application.Abstractions;
using TaskBoard.Application.Common;

namespace TaskBoard.Application.Tasks;

/// <summary>Replaces the editable fields of an owned task. Expects a request validated by <see cref="UpdateTaskRequestValidator"/>.</summary>
public sealed class UpdateTask(ITaskRepository tasks, ICurrentUser currentUser, TimeProvider time)
{
    public async Task<Result<TaskDto>> ExecuteAsync(Guid id, UpdateTaskRequest request, CancellationToken cancellationToken)
    {
        var task = await tasks.GetForUserAsync(id, currentUser.UserId, cancellationToken);
        if (task is null)
        {
            return TaskErrors.NotFound;
        }

        var timeZone = currentUser.GetTimeZone();
        var requestedLocal = request.DueDate!.Value;
        DateTime dueDateUtc;
        if (requestedLocal == DueDates.ToLocal(task.DueDateUtc, timeZone))
        {
            // Unchanged in the form: keep the stored instant. Re-converting an ambiguous local time
            // (DST fall-back) could otherwise shift it by an hour on an unrelated edit.
            dueDateUtc = task.DueDateUtc;
        }
        else
        {
            var converted = DueDates.ToUtc(requestedLocal, timeZone);
            if (!converted.IsSuccess)
            {
                return converted.Error!;
            }

            dueDateUtc = converted.Value;
        }

        task.UpdateDetails(request.Title, request.Description, dueDateUtc, request.Status!.Value);
        await tasks.UpdateAsync(task, cancellationToken);

        return TaskDto.From(task, timeZone, time.GetUtcNow().UtcDateTime);
    }
}
