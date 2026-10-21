import { CdkDrag, CdkDragDrop, CdkDragPlaceholder, CdkDropList } from '@angular/cdk/drag-drop';
import { Component, input, output } from '@angular/core';

import { TaskDto, TaskItemStatus } from '../../../../core/api/generated/api-client';
import { StatusColumn } from '../../task-status';
import { TaskCard } from '../task-card/task-card';

export interface TaskMove {
  task: TaskDto;
  status: TaskItemStatus;
}

@Component({
  selector: 'app-task-column',
  imports: [CdkDropList, CdkDrag, CdkDragPlaceholder, TaskCard],
  templateUrl: './task-column.html',
  styleUrl: './task-column.scss',
})
export class TaskColumn {
  readonly column = input.required<StatusColumn>();
  readonly tasks = input.required<TaskDto[]>();
  readonly isPending = input.required<(id: string) => boolean>();

  readonly moved = output<TaskMove>();
  readonly opened = output<TaskDto>();
  readonly deleteRequested = output<TaskDto>();

  private dragging = false;

  /**
   * Only moves between columns are persisted; there is no saved order inside a column (sorting is disabled
   * on the drop list), so dropping a card back into its own column does nothing.
   */
  protected drop(event: CdkDragDrop<TaskItemStatus, TaskItemStatus, TaskDto>): void {
    if (event.previousContainer !== event.container) {
      this.moved.emit({ task: event.item.data, status: event.container.data });
    }
  }

  protected dragStarted(): void {
    this.dragging = true;
  }

  // Some browsers fire a click on the card right after a drop; ignore it so the edit modal doesn't open.
  protected dragEnded(): void {
    setTimeout(() => (this.dragging = false));
  }

  protected open(task: TaskDto): void {
    if (!this.dragging) {
      this.opened.emit(task);
    }
  }
}
