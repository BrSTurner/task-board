namespace TaskBoard.Domain.Enums;

/// <summary>Kanban column of a task. Named to avoid clashing with <see cref="System.Threading.Tasks.TaskStatus"/>.</summary>
public enum TaskItemStatus
{
    Todo = 0,
    InProgress = 1,
    Done = 2,
}
