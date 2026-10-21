import { TaskDto } from '../../core/api/generated/api-client';

/**
 * Same rule as the API's `TaskOrdering` (BR-007): overdue first (earliest due first), then due date, title, id.
 * Re-applied locally so columns stay ordered after create/edit/move without reloading the board.
 * Dates are compared as instants (`Date.parse`), never as strings — "…:00.5Z" sorts before "…:00Z" as text.
 */
export function compareTasks(a: TaskDto, b: TaskDto): number {
  return (
    Number(b.isOverdue) - Number(a.isOverdue) ||
    Date.parse(a.dueDateUtc) - Date.parse(b.dueDateUtc) ||
    a.title.localeCompare(b.title, undefined, { sensitivity: 'base' }) ||
    a.id.localeCompare(b.id)
  );
}

export function sortTasks(tasks: readonly TaskDto[]): TaskDto[] {
  return [...tasks].sort(compareTasks);
}
