/**
 * Due dates arrive from the API as `dueDateLocal`: a wall-clock string already converted to the user's
 * configured timezone (e.g. "2026-10-21T15:00:00"). These helpers only reshape that string — they never create
 * `Date` objects, so the browser's own timezone can never shift the value (BR-008, Scenario 22).
 */
const WALL_CLOCK = /^(\d{4}-\d{2}-\d{2})T(\d{2}:\d{2})/;

/** "2026-10-21T15:00:00" → "2026-10-21 15:00" (the display format from Scenario 22). */
export function formatLocalDateTime(value: string | null | undefined): string {
  const match = value ? WALL_CLOCK.exec(value) : null;
  return match ? `${match[1]} ${match[2]}` : '';
}

/** "2026-10-21T15:00:00" → "2026-10-21T15:00", the value format of `<input type="datetime-local">`. */
export function toDateTimeLocalInput(value: string | null | undefined): string {
  const match = value ? WALL_CLOCK.exec(value) : null;
  return match ? `${match[1]}T${match[2]}` : '';
}
