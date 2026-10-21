import { sortTasks } from './task-ordering';
import { aTask } from './testing/task-fixtures';

describe('sortTasks (BR-007)', () => {
  it('puts overdue tasks first, earliest due first, then the rest by due date', () => {
    const futureB = aTask({ title: 'Future B', dueDateUtc: '2099-02-01T00:00:00Z' });
    const overdueLate = aTask({ title: 'Overdue late', dueDateUtc: '2020-02-01T00:00:00Z', isOverdue: true });
    const futureA = aTask({ title: 'Future A', dueDateUtc: '2099-01-01T00:00:00Z' });
    const overdueEarly = aTask({ title: 'Overdue early', dueDateUtc: '2020-01-01T00:00:00Z', isOverdue: true });

    expect(sortTasks([futureB, overdueLate, futureA, overdueEarly]).map((t) => t.title)).toEqual([
      'Overdue early',
      'Overdue late',
      'Future A',
      'Future B',
    ]);
  });

  it('compares due dates as instants, not strings', () => {
    const fractional = aTask({ title: 'Later', dueDateUtc: '2099-01-01T00:00:00.5Z' });
    const whole = aTask({ title: 'Earlier', dueDateUtc: '2099-01-01T00:00:00Z' });

    expect(sortTasks([fractional, whole]).map((t) => t.title)).toEqual(['Earlier', 'Later']);
  });

  it('breaks ties by title, case-insensitively', () => {
    const due = '2099-01-01T00:00:00Z';

    expect(sortTasks([aTask({ title: 'beta', dueDateUtc: due }), aTask({ title: 'Alpha', dueDateUtc: due })]).map((t) => t.title))
      .toEqual(['Alpha', 'beta']);
  });
});
