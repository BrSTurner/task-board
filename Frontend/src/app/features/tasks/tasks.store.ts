import { Injectable, computed, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';

import {
  CreateTaskRequest,
  TaskDto,
  TaskItemStatus,
  TasksClient,
  UpdateTaskRequest,
} from '../../core/api/generated/api-client';
import { apiErrorCode } from '../../core/errors/api-error';
import { sortTasks } from './task-ordering';

const TASK_NOT_FOUND = 'Task.NotFound';

/**
 * Board state. Provided by the board page (not root), so it is destroyed on logout/navigation and one user's
 * tasks can never leak into another user's session in the same tab.
 * Errors are displayed by the HTTP error interceptor; the store only keeps its state consistent.
 */
@Injectable()
export class TasksStore {
  private readonly client = inject(TasksClient);

  private readonly tasks = signal<readonly TaskDto[]>([]);
  private readonly pendingIds = signal<ReadonlySet<string>>(new Set());
  private readonly loadState = signal<'idle' | 'loading' | 'loaded' | 'failed'>('idle');

  readonly loading = computed(() => this.loadState() === 'loading');
  readonly loadFailed = computed(() => this.loadState() === 'failed');
  readonly isEmpty = computed(() => this.loadState() === 'loaded' && this.tasks().length === 0);

  readonly columns = computed<Record<TaskItemStatus, TaskDto[]>>(() => {
    const tasks = this.tasks();
    return {
      todo: sortTasks(tasks.filter((t) => t.status === 'todo')),
      inProgress: sortTasks(tasks.filter((t) => t.status === 'inProgress')),
      done: sortTasks(tasks.filter((t) => t.status === 'done')),
    };
  });

  isPending(id: string): boolean {
    return this.pendingIds().has(id);
  }

  async load(): Promise<void> {
    this.loadState.set('loading');
    try {
      const result = await firstValueFrom(this.client.getMyTasks());
      this.tasks.set(result.data ?? []);
      this.loadState.set('loaded');
    } catch {
      this.loadState.set('failed');
    }
  }

  async create(request: CreateTaskRequest): Promise<TaskDto> {
    const result = await firstValueFrom(this.client.createTask(request));
    const created = result.data as TaskDto;
    this.tasks.update((tasks) => [...tasks, created]);
    return created;
  }

  async update(id: string, request: UpdateTaskRequest): Promise<TaskDto> {
    try {
      const result = await firstValueFrom(this.client.updateTask(id, request));
      const updated = result.data as TaskDto;
      this.replace(updated);
      return updated;
    } catch (error) {
      this.forgetIfGone(id, error);
      throw error;
    }
  }

  /**
   * Optimistic move: the card changes column immediately and is restored if the API rejects the change.
   * The server response then replaces the card, carrying the recomputed `isOverdue`.
   */
  async move(id: string, status: TaskItemStatus): Promise<TaskDto | null> {
    const previous = this.tasks().find((t) => t.id === id);
    if (!previous || previous.status === status || this.isPending(id)) {
      return null;
    }

    // Done tasks are never overdue (BR-006); other moves keep the flag until the server answers.
    this.replace({ ...previous, status, isOverdue: status === 'done' ? false : previous.isOverdue });
    this.setPending(id, true);
    try {
      const result = await firstValueFrom(this.client.changeTaskStatus(id, { status }));
      const moved = result.data as TaskDto;
      this.replace(moved);
      return moved;
    } catch (error) {
      this.replace(previous);
      this.forgetIfGone(id, error);
      throw error;
    } finally {
      this.setPending(id, false);
    }
  }

  /** Removes the card only after the API confirms the deletion. */
  async remove(id: string): Promise<void> {
    this.setPending(id, true);
    try {
      await firstValueFrom(this.client.deleteTask(id));
      this.tasks.update((tasks) => tasks.filter((t) => t.id !== id));
    } catch (error) {
      this.forgetIfGone(id, error);
      throw error;
    } finally {
      this.setPending(id, false);
    }
  }

  private replace(task: TaskDto): void {
    this.tasks.update((tasks) => tasks.map((t) => (t.id === task.id ? task : t)));
  }

  /** A 404 means the task was deleted elsewhere (e.g. another tab): drop the stale card. */
  private forgetIfGone(id: string, error: unknown): void {
    if (apiErrorCode(error) === TASK_NOT_FOUND) {
      this.tasks.update((tasks) => tasks.filter((t) => t.id !== id));
    }
  }

  private setPending(id: string, pending: boolean): void {
    this.pendingIds.update((ids) => {
      const next = new Set(ids);
      if (pending) {
        next.add(id);
      } else {
        next.delete(id);
      }
      return next;
    });
  }
}
