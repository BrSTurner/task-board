import { Dialog } from '@angular/cdk/dialog';
import { CdkDropListGroup } from '@angular/cdk/drag-drop';
import { Component, OnInit, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ToastrService } from 'ngx-toastr';

import { TaskDto, TaskItemStatus } from '../../../../core/api/generated/api-client';
import { AuthStore } from '../../../../core/auth/auth.store';
import { ConfirmDialog, ConfirmDialogData } from '../../../../shared/components/confirm-dialog/confirm-dialog';
import { TaskColumn, TaskMove } from '../../components/task-column/task-column';
import { TaskFormDialog, TaskFormDialogData, TaskFormValue } from '../../components/task-form-dialog/task-form-dialog';
import { STATUS_COLUMNS, statusTitle } from '../../task-status';
import { TasksStore } from '../../tasks.store';

const DIALOG_PANEL = 'tb-dialog-panel';

@Component({
  selector: 'app-task-board',
  imports: [CdkDropListGroup, TaskColumn],
  providers: [TasksStore],
  templateUrl: './task-board.html',
  styleUrl: './task-board.scss',
})
export class TaskBoardPage implements OnInit {
  protected readonly store = inject(TasksStore);
  protected readonly auth = inject(AuthStore);
  private readonly dialog = inject(Dialog);
  private readonly toastr = inject(ToastrService);

  protected readonly columns = STATUS_COLUMNS;
  protected readonly timeZone = this.auth.user()?.timeZone ?? 'UTC';
  /** Polite screen-reader announcement for moves (drag-and-drop gives no other non-visual feedback). */
  protected readonly announcement = signal('');
  protected readonly isPending = (id: string) => this.store.isPending(id);

  ngOnInit(): void {
    void this.store.load();
  }

  protected reload(): void {
    void this.store.load();
  }

  protected openCreate(): void {
    this.openForm({
      timeZone: this.timeZone,
      save: async (value) => {
        await this.store.create({ title: value.title, description: value.description, dueDate: value.dueDate });
        this.toastr.success('Task created');
      },
    });
  }

  protected openEdit(task: TaskDto): void {
    this.openForm({
      task,
      timeZone: this.timeZone,
      save: async (value: TaskFormValue) => {
        await this.store.update(task.id, value);
        this.toastr.success('Task updated');
      },
    });
  }

  protected async move({ task, status }: TaskMove): Promise<void> {
    try {
      const moved = await this.store.move(task.id, status);
      if (moved) {
        this.announce(`${moved.title} moved to ${statusTitle(status)}`);
        this.toastr.success(`Moved to ${statusTitle(status)}`, 'Task updated');
      }
    } catch {
      this.announce(`${task.title} could not be moved`);
    }
  }

  protected async confirmDelete(task: TaskDto): Promise<void> {
    const ref = this.dialog.open<boolean, ConfirmDialogData>(ConfirmDialog, {
      data: { title: 'Delete task?', message: `"${task.title}" will be permanently deleted.`, confirmLabel: 'Delete' },
      panelClass: DIALOG_PANEL,
    });
    if (!(await firstValueFrom(ref.closed))) {
      return;
    }

    try {
      await this.store.remove(task.id);
      this.toastr.success('Task deleted');
    } catch {
      // Shown by the error interceptor; the card stays on the board.
    }
  }

  protected columnTasks(status: TaskItemStatus): TaskDto[] {
    return this.store.columns()[status];
  }

  private openForm(data: TaskFormDialogData): void {
    this.dialog.open<boolean, TaskFormDialogData>(TaskFormDialog, { data, panelClass: DIALOG_PANEL, disableClose: true });
  }

  private announce(message: string): void {
    this.announcement.set(message);
  }
}
