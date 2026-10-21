import { CdkDragDrop } from '@angular/cdk/drag-drop';
import { TestBed } from '@angular/core/testing';

import { TaskDto, TaskItemStatus } from '../../../../core/api/generated/api-client';
import { aTask } from '../../testing/task-fixtures';
import { TaskColumn, TaskMove } from './task-column';

describe('TaskColumn drop handling', () => {
  // jsdom cannot simulate pointer drags, so the CDK drop event is constructed directly.
  function dropEvent(task: TaskDto, from: TaskItemStatus, to: TaskItemStatus): CdkDragDrop<TaskItemStatus, TaskItemStatus, TaskDto> {
    const previousContainer = { data: from };
    const container = from === to ? previousContainer : { data: to };
    return { item: { data: task }, previousContainer, container } as unknown as CdkDragDrop<TaskItemStatus, TaskItemStatus, TaskDto>;
  }

  function createColumn() {
    const fixture = TestBed.createComponent(TaskColumn);
    fixture.componentRef.setInput('column', { status: 'inProgress', title: 'In Progress' });
    fixture.componentRef.setInput('tasks', []);
    fixture.componentRef.setInput('isPending', () => false);
    const moves: TaskMove[] = [];
    fixture.componentInstance.moved.subscribe((move) => moves.push(move));
    type DropEvent = CdkDragDrop<TaskItemStatus, TaskItemStatus, TaskDto>;
    const drop = (event: DropEvent): void =>
      (fixture.componentInstance as unknown as { drop: (event: DropEvent) => void }).drop(event);
    return { drop, moves };
  }

  it('emits a move with the target column status when dropped from another column (Scenario 14)', () => {
    const { drop, moves } = createColumn();
    const task = aTask({ status: 'todo' });

    drop(dropEvent(task, 'todo', 'inProgress'));

    expect(moves).toEqual([{ task, status: 'inProgress' }]);
  });

  it('ignores drops inside the same column (no persisted manual ordering)', () => {
    const { drop, moves } = createColumn();

    drop(dropEvent(aTask({ status: 'inProgress' }), 'inProgress', 'inProgress'));

    expect(moves).toEqual([]);
  });
});
