import { Component, input, output } from '@angular/core';

import { TaskDto, TaskItemStatus } from '../../../../core/api/generated/api-client';
import { LocalDateTimePipe } from '../../../../shared/utils/local-date-time.pipe';
import { STATUS_COLUMNS } from '../../task-status';

@Component({
  selector: 'app-task-card',
  imports: [LocalDateTimePipe],
  templateUrl: './task-card.html',
  styleUrl: './task-card.scss',
})
export class TaskCard {
  readonly task = input.required<TaskDto>();
  readonly pending = input(false);

  readonly opened = output<void>();
  readonly deleteRequested = output<void>();
  readonly statusChange = output<TaskItemStatus>();

  protected readonly statuses = STATUS_COLUMNS;

  protected changeStatus(event: Event): void {
    const status = (event.target as HTMLSelectElement).value as TaskItemStatus;
    if (status !== this.task().status) {
      this.statusChange.emit(status);
    }
  }
}
