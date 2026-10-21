import { DIALOG_DATA, DialogRef } from '@angular/cdk/dialog';
import { Component, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

import { TaskDto, TaskItemStatus } from '../../../../core/api/generated/api-client';
import { toDateTimeLocalInput } from '../../../../shared/utils/date-time';
import { maxTrimmedLength, notBlank } from '../../../../shared/validators';
import { STATUS_COLUMNS } from '../../task-status';

export const TITLE_MAX_LENGTH = 50;
export const DESCRIPTION_MAX_LENGTH = 400;

export interface TaskFormValue {
  title: string;
  description: string | null;
  /** Local wall-clock "YYYY-MM-DDTHH:mm" in the user's configured timezone (converted to UTC by the API). */
  dueDate: string;
  status: TaskItemStatus;
}

export interface TaskFormDialogData {
  /** Absent when creating. */
  task?: TaskDto;
  timeZone: string;
  /** Persists the value; the dialog stays open (keeping the user's input) if it rejects. */
  save: (value: TaskFormValue) => Promise<unknown>;
}

@Component({
  selector: 'app-task-form-dialog',
  imports: [ReactiveFormsModule],
  templateUrl: './task-form-dialog.html',
  // The dialog is opened with disableClose (a stray backdrop click must not discard input), so Escape is explicit.
  host: { '(keydown.escape)': 'cancel()' },
})
export class TaskFormDialog {
  protected readonly data = inject<TaskFormDialogData>(DIALOG_DATA);
  private readonly dialogRef = inject<DialogRef<boolean>>(DialogRef);

  protected readonly isEdit = this.data.task !== undefined;
  protected readonly statuses = STATUS_COLUMNS;
  protected readonly titleMax = TITLE_MAX_LENGTH;
  protected readonly descriptionMax = DESCRIPTION_MAX_LENGTH;

  // No HTML maxlength attributes: typing past the limit must show the red error (Scenario 11).
  protected readonly form = inject(NonNullableFormBuilder).group({
    title: [this.data.task?.title ?? '', [notBlank, maxTrimmedLength(TITLE_MAX_LENGTH)]],
    description: [this.data.task?.description ?? '', [maxTrimmedLength(DESCRIPTION_MAX_LENGTH)]],
    dueDate: [toDateTimeLocalInput(this.data.task?.dueDateLocal), [Validators.required]],
    status: [this.data.task?.status ?? ('todo' as TaskItemStatus)],
  });

  protected readonly saving = signal(false);
  protected readonly submitted = signal(false);
  protected readonly titleLength = toSignal(this.form.controls.title.valueChanges, { initialValue: this.form.controls.title.value });
  protected readonly descriptionLength = toSignal(this.form.controls.description.valueChanges, {
    initialValue: this.form.controls.description.value,
  });

  protected invalid(field: 'title' | 'description' | 'dueDate'): boolean {
    const control = this.form.controls[field];
    return control.invalid && (control.touched || this.submitted());
  }

  protected titleError(): string {
    return this.form.controls.title.hasError('required') ? 'Title is required' : `Title must be at most ${TITLE_MAX_LENGTH} characters`;
  }

  protected async submit(): Promise<void> {
    this.submitted.set(true);
    this.form.markAllAsTouched();
    if (this.form.invalid || this.saving()) {
      return;
    }

    const raw = this.form.getRawValue();
    const description = raw.description.trim();
    this.saving.set(true);
    try {
      await this.data.save({ title: raw.title.trim(), description: description || null, dueDate: raw.dueDate, status: raw.status });
      this.dialogRef.close(true);
    } catch {
      // The error modal explains what went wrong; keep the dialog open so no input is lost.
    } finally {
      this.saving.set(false);
    }
  }

  protected cancel(): void {
    this.dialogRef.close(false);
  }
}
