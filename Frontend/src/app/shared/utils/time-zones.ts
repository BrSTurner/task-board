/** The browser's IANA timezone, used as the registration default. */
export function browserTimeZone(): string {
  try {
    return Intl.DateTimeFormat().resolvedOptions().timeZone ?? '';
  } catch {
    return '';
  }
}

/**
 * IANA timezone ids offered at registration. The browser's resolved zone can be a legacy alias that is missing
 * from `supportedValuesOf` (e.g. "Asia/Calcutta"), so it is added explicitly; the backend validates the id.
 */
export function availableTimeZones(): string[] {
  const supported = typeof Intl.supportedValuesOf === 'function' ? Intl.supportedValuesOf('timeZone') : [];
  const zones = new Set<string>([...supported, 'UTC']);
  const browser = browserTimeZone();
  if (browser) {
    zones.add(browser);
  }

  return [...zones].sort((a, b) => a.localeCompare(b));
}
