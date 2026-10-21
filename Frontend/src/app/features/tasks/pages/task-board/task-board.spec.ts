import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ToastrService } from 'ngx-toastr';
import { of } from 'rxjs';

import { TasksClient } from '../../../../core/api/generated/api-client';
import { AuthStore } from '../../../../core/auth/auth.store';
import { aTask, ok } from '../../testing/task-fixtures';
import { TaskBoardPage } from './task-board';

describe('TaskBoardPage', () => {
  let fixture: ComponentFixture<TaskBoardPage>;
  let element: HTMLElement;
  let client: { getMyTasks: ReturnType<typeof vi.fn>; changeTaskStatus: ReturnType<typeof vi.fn> };
  let toastr: { success: ReturnType<typeof vi.fn> };

  async function render(tasks: ReturnType<typeof aTask>[]): Promise<void> {
    client = { getMyTasks: vi.fn().mockReturnValue(of(ok(tasks))), changeTaskStatus: vi.fn() };
    toastr = { success: vi.fn() };
    await TestBed.configureTestingModule({
      imports: [TaskBoardPage],
      providers: [
        { provide: TasksClient, useValue: client },
        { provide: ToastrService, useValue: toastr },
        {
          provide: AuthStore,
          useValue: {
            user: signal({ id: 'u1', name: 'Alice', email: 'demo@example.test', timeZone: 'America/Sao_Paulo' }),
            logout: vi.fn(),
          },
        },
      ],
    }).compileComponents();
    fixture = TestBed.createComponent(TaskBoardPage);
    element = fixture.nativeElement;
    await fixture.whenStable();
  }

  const columnTitles = (status: string) =>
    [...element.querySelectorAll(`[data-status="${status}"] .card-title`)].map((e) => e.textContent?.trim());

  it('shows exactly the three Kanban columns with an empty-board call to action (Scenarios 6 and 7)', async () => {
    await render([]);

    expect([...element.querySelectorAll('app-task-column h2')].map((h) => h.textContent?.trim())).toEqual([
      'To Do',
      'In Progress',
      'Done',
    ]);
    expect(element.textContent).toContain('Create your first task');
    expect(element.querySelectorAll('.task-column__empty')).toHaveLength(3);
    expect(element.textContent).toContain('Times shown in America/Sao_Paulo');
  });

  it('places tasks in their columns with overdue cards first and a red Overdue pill (Scenarios 17–19)', async () => {
    await render([
      aTask({ title: 'Future', status: 'inProgress' }),
      aTask({ title: 'Late', status: 'inProgress', isOverdue: true, dueDateUtc: '2020-01-01T00:00:00Z' }),
      aTask({ title: 'Finished', status: 'done', dueDateUtc: '2020-01-01T00:00:00Z' }),
    ]);

    expect(columnTitles('inProgress')).toEqual(['Late', 'Future']);
    expect(columnTitles('done')).toEqual(['Finished']);
    const pills = [...element.querySelectorAll('.badge.text-bg-danger')].map((b) => b.textContent?.trim());
    expect(pills).toEqual(['Overdue']);
    expect(element.textContent).not.toContain('Create your first task');
  });

  it('moves a task through the keyboard-accessible status select and confirms with a toast', async () => {
    const task = aTask({ title: 'Movable', status: 'todo' });
    await render([task]);
    client.changeTaskStatus.mockReturnValue(of(ok({ ...task, status: 'inProgress' })));

    const select = element.querySelector<HTMLSelectElement>('[data-status="todo"] select')!;
    select.value = 'inProgress';
    select.dispatchEvent(new Event('change'));
    await fixture.whenStable();

    expect(client.changeTaskStatus).toHaveBeenCalledWith(task.id, { status: 'inProgress' });
    expect(columnTitles('inProgress')).toEqual(['Movable']);
    expect(toastr.success).toHaveBeenCalled();
  });
});
