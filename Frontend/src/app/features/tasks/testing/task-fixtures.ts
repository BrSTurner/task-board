import { TaskDto } from '../../../core/api/generated/api-client';

let sequence = 0;

export function aTask(overrides: Partial<TaskDto> = {}): TaskDto {
  sequence += 1;
  return {
    id: `00000000-0000-7000-8000-${String(sequence).padStart(12, '0')}`,
    title: `Task ${sequence}`,
    description: null,
    status: 'todo',
    dueDateUtc: '2099-01-01T12:00:00Z',
    dueDateLocal: '2099-01-01T09:00:00',
    isOverdue: false,
    ...overrides,
  };
}

/** What a generated client emits/throws for an envelope response. */
export const ok = <T>(data: T) => ({ isSuccess: true, data, error: null });
export const apiFailure = (code: string, message = code) => ({ isSuccess: false, error: { code, message } });
