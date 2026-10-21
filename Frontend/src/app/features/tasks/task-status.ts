import { TaskItemStatus } from '../../core/api/generated/api-client';

export interface StatusColumn {
  status: TaskItemStatus;
  title: string;
}

/** The three Kanban columns, in board order (Scenario 6). */
export const STATUS_COLUMNS: readonly StatusColumn[] = [
  { status: 'todo', title: 'To Do' },
  { status: 'inProgress', title: 'In Progress' },
  { status: 'done', title: 'Done' },
];

export function statusTitle(status: TaskItemStatus): string {
  return STATUS_COLUMNS.find((column) => column.status === status)?.title ?? status;
}
