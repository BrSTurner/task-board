using TaskBoard.Application.Abstractions;
using TaskBoard.Application.Common;

namespace TaskBoard.Application.Tasks;

// TDD stubs — each use case is implemented (and moved to its own file) in the GREEN step.

public sealed class GetMyTasks(ITaskRepository tasks, ICurrentUser currentUser, TimeProvider time)
{
    public Task<Result<IReadOnlyList<TaskDto>>> ExecuteAsync(CancellationToken cancellationToken)
    {
        _ = (tasks, currentUser, time);
        return Task.FromResult<Result<IReadOnlyList<TaskDto>>>(Error.Conflict("Stub", "TDD stub"));
    }
}

public sealed class GetTaskById(ITaskRepository tasks, ICurrentUser currentUser, TimeProvider time)
{
    public Task<Result<TaskDto>> ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        _ = (tasks, currentUser, time);
        return Task.FromResult<Result<TaskDto>>(Error.Conflict("Stub", "TDD stub"));
    }
}

public sealed class CreateTask(ITaskRepository tasks, IUserRepository users, ICurrentUser currentUser, TimeProvider time)
{
    public Task<Result<TaskDto>> ExecuteAsync(CreateTaskRequest request, CancellationToken cancellationToken)
    {
        _ = (tasks, users, currentUser, time);
        return Task.FromResult<Result<TaskDto>>(Error.Conflict("Stub", "TDD stub"));
    }
}

public sealed class UpdateTask(ITaskRepository tasks, ICurrentUser currentUser, TimeProvider time)
{
    public Task<Result<TaskDto>> ExecuteAsync(Guid id, UpdateTaskRequest request, CancellationToken cancellationToken)
    {
        _ = (tasks, currentUser, time);
        return Task.FromResult<Result<TaskDto>>(Error.Conflict("Stub", "TDD stub"));
    }
}

public sealed class ChangeTaskStatus(ITaskRepository tasks, ICurrentUser currentUser, TimeProvider time)
{
    public Task<Result<TaskDto>> ExecuteAsync(Guid id, ChangeTaskStatusRequest request, CancellationToken cancellationToken)
    {
        _ = (tasks, currentUser, time);
        return Task.FromResult<Result<TaskDto>>(Error.Conflict("Stub", "TDD stub"));
    }
}

public sealed class DeleteTask(ITaskRepository tasks, ICurrentUser currentUser)
{
    public Task<Result> ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        _ = (tasks, currentUser);
        return Task.FromResult<Result>(Error.Conflict("Stub", "TDD stub"));
    }
}
