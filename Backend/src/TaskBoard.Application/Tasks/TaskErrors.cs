using TaskBoard.Application.Common;

namespace TaskBoard.Application.Tasks;

public static class TaskErrors
{
    /// <summary>Used for both absent and not-owned tasks (Scenario 24).</summary>
    public static readonly Error NotFound = Error.NotFound("Task.NotFound", "The task was not found.");
}
