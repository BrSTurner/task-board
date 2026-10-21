import { TestBed } from '@angular/core/testing';
import { Observable, Subject, of, throwError } from 'rxjs';

import { TasksClient } from '../../core/api/generated/api-client';
import { TasksStore } from './tasks.store';
import { aTask, apiFailure, ok } from './testing/task-fixtures';

describe('TasksStore', () => {
  let client: {
    getMyTasks: ReturnType<typeof vi.fn>;
    createTask: ReturnType<typeof vi.fn>;
    updateTask: ReturnType<typeof vi.fn>;
    changeTaskStatus: ReturnType<typeof vi.fn>;
    deleteTask: ReturnType<typeof vi.fn>;
  };
  let store: TasksStore;

  beforeEach(() => {
    client = {
      getMyTasks: vi.fn(),
      createTask: vi.fn(),
      updateTask: vi.fn(),
      changeTaskStatus: vi.fn(),
      deleteTask: vi.fn(),
    };
    TestBed.configureTestingModule({ providers: [TasksStore, { provide: TasksClient, useValue: client }] });
    store = TestBed.inject(TasksStore);
  });

  async function loadWith(...tasks: ReturnType<typeof aTask>[]): Promise<void> {
    client.getMyTasks.mockReturnValue(of(ok(tasks)));
    await store.load();
  }

  it('groups loaded tasks into the three status columns, overdue first', async () => {
    const todoFuture = aTask({ title: 'Todo future' });
    const todoOverdue = aTask({ title: 'Todo overdue', isOverdue: true, dueDateUtc: '2020-01-01T00:00:00Z' });
    const inProgress = aTask({ title: 'Doing', status: 'inProgress' });
    const done = aTask({ title: 'Finished', status: 'done' });

    await loadWith(todoFuture, inProgress, todoOverdue, done);

    expect(store.columns().todo.map((t) => t.title)).toEqual(['Todo overdue', 'Todo future']);
    expect(store.columns().inProgress.map((t) => t.title)).toEqual(['Doing']);
    expect(store.columns().done.map((t) => t.title)).toEqual(['Finished']);
    expect(store.isEmpty()).toBe(false);
  });

  it('reports an empty board and load failures', async () => {
    await loadWith();
    expect(store.isEmpty()).toBe(true);

    client.getMyTasks.mockReturnValue(throwError(() => apiFailure('ServerError')));
    await store.load();
    expect(store.loadFailed()).toBe(true);
  });

  it('adds a created task to the To Do column', async () => {
    await loadWith();
    const created = aTask({ title: 'New' });
    client.createTask.mockReturnValue(of(ok(created)));

    await store.create({ title: 'New', description: null, dueDate: '2099-01-01T09:00' });

    expect(store.columns().todo).toEqual([created]);
  });

  it('moves a card optimistically and then applies the server response', async () => {
    const task = aTask({ title: 'Late', status: 'inProgress', isOverdue: true });
    await loadWith(task);
    const response = new Subject<unknown>();
    client.changeTaskStatus.mockReturnValue(response as Observable<unknown>);

    const moving = store.move(task.id, 'done');

    // Optimistic: already in Done, overdue pill gone, card locked against a second drag.
    expect(store.columns().done.map((t) => t.id)).toEqual([task.id]);
    expect(store.columns().done[0].isOverdue).toBe(false);
    expect(store.isPending(task.id)).toBe(true);

    response.next(ok({ ...task, status: 'done', isOverdue: false, title: 'Late (server)' }));
    response.complete();
    await moving;

    expect(client.changeTaskStatus).toHaveBeenCalledWith(task.id, { status: 'done' });
    expect(store.columns().done[0].title).toBe('Late (server)');
    expect(store.isPending(task.id)).toBe(false);
  });

  it('rolls the card back when the move fails', async () => {
    const task = aTask({ status: 'todo' });
    await loadWith(task);
    client.changeTaskStatus.mockReturnValue(throwError(() => apiFailure('ServerError')));

    await expect(store.move(task.id, 'inProgress')).rejects.toBeTruthy();

    expect(store.columns().todo.map((t) => t.id)).toEqual([task.id]);
    expect(store.columns().inProgress).toEqual([]);
    expect(store.isPending(task.id)).toBe(false);
  });

  it('drops a card that no longer exists on the server (404)', async () => {
    const task = aTask();
    await loadWith(task);
    client.changeTaskStatus.mockReturnValue(throwError(() => apiFailure('Task.NotFound')));

    await expect(store.move(task.id, 'done')).rejects.toBeTruthy();

    expect(store.columns().todo).toEqual([]);
    expect(store.columns().done).toEqual([]);
  });

  it('ignores a move to the same column', async () => {
    const task = aTask({ status: 'todo' });
    await loadWith(task);

    expect(await store.move(task.id, 'todo')).toBeNull();
    expect(client.changeTaskStatus).not.toHaveBeenCalled();
  });

  it('removes a card only after the delete is confirmed by the API', async () => {
    const task = aTask();
    await loadWith(task);
    const response = new Subject<void>();
    client.deleteTask.mockReturnValue(response);

    const removing = store.remove(task.id);
    expect(store.columns().todo).toHaveLength(1);

    response.next();
    response.complete();
    await removing;
    expect(store.columns().todo).toHaveLength(0);
  });

  it('keeps the card when the delete fails', async () => {
    const task = aTask();
    await loadWith(task);
    client.deleteTask.mockReturnValue(throwError(() => apiFailure('ServerError')));

    await expect(store.remove(task.id)).rejects.toBeTruthy();

    expect(store.columns().todo).toHaveLength(1);
  });
});
