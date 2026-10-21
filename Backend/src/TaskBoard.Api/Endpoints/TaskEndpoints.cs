using TaskBoard.Api.Http;
using TaskBoard.Application.Tasks;

namespace TaskBoard.Api.Endpoints;

public static class TaskEndpoints
{
    public static IEndpointRouteBuilder MapTaskEndpoints(this IEndpointRouteBuilder app)
    {
        // Every task operation requires a bearer token and is scoped to the authenticated user.
        var group = app.MapGroup("/api/tasks")
            .WithTags("Tasks")
            .RequireAuthorization()
            .ProducesApiError(StatusCodes.Status401Unauthorized);

        group.MapGet("/", async (GetMyTasks getMyTasks, CancellationToken cancellationToken) =>
                (await getMyTasks.ExecuteAsync(cancellationToken)).ToHttpResult(ApiResults.Ok))
            .WithName("GetMyTasks")
            .WithSummary("Lists the current user's tasks, overdue first.")
            .Produces<ApiResult<IReadOnlyList<TaskDto>>>();

        group.MapGet("/{id:guid}", async (Guid id, GetTaskById getTaskById, CancellationToken cancellationToken) =>
                (await getTaskById.ExecuteAsync(id, cancellationToken)).ToHttpResult(ApiResults.Ok))
            .WithName("GetTaskById")
            .Produces<ApiResult<TaskDto>>()
            .ProducesApiError(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateTaskRequest request, CreateTask createTask, CancellationToken cancellationToken) =>
                (await createTask.ExecuteAsync(request, cancellationToken))
                    .ToHttpResult(task => TypedResults.Created($"/api/tasks/{task.Id}", ApiResult.Success(task))))
            .WithValidation<CreateTaskRequest>()
            .WithName("CreateTask")
            .WithSummary("Creates a task in To Do. dueDate is local wall-clock time in the user's timezone.")
            .Produces<ApiResult<TaskDto>>(StatusCodes.Status201Created);

        group.MapPut("/{id:guid}", async (Guid id, UpdateTaskRequest request, UpdateTask updateTask, CancellationToken cancellationToken) =>
                (await updateTask.ExecuteAsync(id, request, cancellationToken)).ToHttpResult(ApiResults.Ok))
            .WithValidation<UpdateTaskRequest>()
            .WithName("UpdateTask")
            .WithSummary("Replaces the task's editable fields.")
            .Produces<ApiResult<TaskDto>>()
            .ProducesApiError(StatusCodes.Status404NotFound);

        group.MapPatch("/{id:guid}/status", async (Guid id, ChangeTaskStatusRequest request, ChangeTaskStatus changeTaskStatus, CancellationToken cancellationToken) =>
                (await changeTaskStatus.ExecuteAsync(id, request, cancellationToken)).ToHttpResult(ApiResults.Ok))
            .WithValidation<ChangeTaskStatusRequest>()
            .WithName("ChangeTaskStatus")
            .WithSummary("Moves the task to another Kanban column.")
            .Produces<ApiResult<TaskDto>>()
            .ProducesApiError(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", async (Guid id, DeleteTask deleteTask, CancellationToken cancellationToken) =>
                (await deleteTask.ExecuteAsync(id, cancellationToken)).ToHttpResult(() => TypedResults.NoContent()))
            .WithName("DeleteTask")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesApiError(StatusCodes.Status404NotFound);

        return app;
    }
}
