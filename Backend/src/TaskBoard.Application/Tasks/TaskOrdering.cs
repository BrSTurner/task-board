namespace TaskBoard.Application.Tasks;

public static class TaskOrdering
{
    // TDD stub — implemented in the GREEN step.
    public static IReadOnlyList<TaskDto> Order(IEnumerable<TaskDto> tasks) => tasks.ToList();
}
