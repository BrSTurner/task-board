import { DIALOG_DATA, DialogRef } from '@angular/cdk/dialog';
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { aTask } from '../../testing/task-fixtures';
import { TaskFormDialog, TaskFormDialogData } from './task-form-dialog';

describe('TaskFormDialog', () => {
  let fixture: ComponentFixture<TaskFormDialog>;
  let element: HTMLElement;
  let dialogRef: { close: ReturnType<typeof vi.fn> };
  let save: ReturnType<typeof vi.fn>;

  async function open(data: Partial<TaskFormDialogData> = {}): Promise<void> {
    dialogRef = { close: vi.fn() };
    save = vi.fn().mockResolvedValue(undefined);
    await TestBed.configureTestingModule({
      imports: [TaskFormDialog],
      providers: [
        { provide: DIALOG_DATA, useValue: { timeZone: 'America/Sao_Paulo', save, ...data } },
        { provide: DialogRef, useValue: dialogRef },
      ],
    }).compileComponents();
    fixture = TestBed.createComponent(TaskFormDialog);
    element = fixture.nativeElement;
    await fixture.whenStable();
  }

  function setValue(selector: string, value: string): void {
    const input = element.querySelector<HTMLInputElement | HTMLTextAreaElement>(selector)!;
    input.value = value;
    input.dispatchEvent(new Event('input'));
  }

  async function submit(): Promise<void> {
    element.querySelector<HTMLButtonElement>('button[type=submit]')!.click();
    await fixture.whenStable();
  }

  it('shows "Title is required" and "Due date is required" and does not save (Scenario 10)', async () => {
    await open();

    await submit();

    expect(element.querySelector('#task-title')!.classList).toContain('is-invalid');
    expect(element.querySelector('#task-due')!.classList).toContain('is-invalid');
    expect(element.textContent).toContain('Title is required');
    expect(element.textContent).toContain('Due date is required');
    expect(save).not.toHaveBeenCalled();
  });

  it('flags titles over 50 and descriptions over 400 characters (Scenario 11)', async () => {
    await open();
    setValue('#task-title', 'x'.repeat(51));
    setValue('#task-description', 'd'.repeat(401));
    setValue('#task-due', '2026-10-21T15:00');

    await submit();

    expect(element.textContent).toContain('Title must be at most 50 characters');
    expect(element.textContent).toContain('Description must be at most 400 characters');
    expect(element.querySelector('#task-title')!.getAttribute('maxlength')).toBeNull(); // typing past the limit is possible
    expect(save).not.toHaveBeenCalled();
  });

  it('creates with trimmed values, an empty description as null and To Do status, then closes', async () => {
    await open();
    setValue('#task-title', '  Prepare technical interview  ');
    setValue('#task-description', '   ');
    setValue('#task-due', '2026-10-21T15:00');

    await submit();

    expect(save).toHaveBeenCalledWith({
      title: 'Prepare technical interview',
      description: null,
      dueDate: '2026-10-21T15:00',
      status: 'todo',
    });
    expect(dialogRef.close).toHaveBeenCalledWith(true);
  });

  it('prefills an existing task in the user timezone and offers the status field', async () => {
    await open({ task: aTask({ title: 'Edit me', status: 'inProgress', dueDateLocal: '2026-10-21T15:00:00' }) });

    expect(element.querySelector<HTMLInputElement>('#task-title')!.value).toBe('Edit me');
    expect(element.querySelector<HTMLInputElement>('#task-due')!.value).toBe('2026-10-21T15:00');
    expect(element.querySelector<HTMLSelectElement>('#task-status')!.value).toBe('inProgress');
    expect(element.textContent).toContain('America/Sao_Paulo');
  });

  it('stays open, keeping the input, when saving fails', async () => {
    await open();
    save.mockRejectedValue(new Error('400'));
    setValue('#task-title', 'Keep me');
    setValue('#task-due', '2026-10-21T15:00');

    await submit();

    expect(dialogRef.close).not.toHaveBeenCalled();
    expect(element.querySelector<HTMLInputElement>('#task-title')!.value).toBe('Keep me');
  });
});
