import { formatLocalDateTime, toDateTimeLocalInput } from './date-time';

describe('date-time utils', () => {
  it('formats the API wall-clock value as "yyyy-MM-dd HH:mm" without timezone conversion', () => {
    expect(formatLocalDateTime('2026-10-21T15:00:00')).toBe('2026-10-21 15:00');
    expect(formatLocalDateTime('2026-03-08T02:30:00')).toBe('2026-03-08 02:30');
  });

  it('produces a datetime-local input value', () => {
    expect(toDateTimeLocalInput('2026-10-21T15:00:00')).toBe('2026-10-21T15:00');
  });

  it('returns an empty string for missing or malformed values', () => {
    expect(formatLocalDateTime(null)).toBe('');
    expect(toDateTimeLocalInput('not a date')).toBe('');
  });
});
