import { HttpErrorResponse } from '@angular/common/http';

import { ApiError, ApiResult } from '../api/generated/api-client';

export interface DisplayError {
  title: string;
  message: string;
  details: string[];
}

const UNREACHABLE: DisplayError = {
  title: 'Server unavailable',
  message: 'The TaskBoard API could not be reached. Check that the backend is running and try again.',
  details: [],
};

/**
 * NSwag clients request `responseType: 'blob'`, so error bodies arrive as a Blob. Reads the `ApiResult`
 * envelope from it; returns `null` for empty or non-JSON bodies (e.g. a dev-proxy 502 page).
 */
export async function readApiError(error: HttpErrorResponse): Promise<ApiError | null> {
  try {
    const body = error.error instanceof Blob ? await readBlobText(error.error) : error.error;
    const parsed: unknown = typeof body === 'string' ? (body ? JSON.parse(body) : null) : body;
    return isApiResult(parsed) && parsed.error ? parsed.error : null;
  } catch {
    return null;
  }
}

export function toDisplayError(status: number, apiError: ApiError | null): DisplayError {
  if (status === 0 || (!apiError && status >= 500)) {
    return UNREACHABLE;
  }

  if (!apiError) {
    return { title: 'Request failed', message: `The request failed (HTTP ${status}).`, details: [] };
  }

  const details = Object.values(apiError.validationErrors ?? {}).flat();
  return { title: titleFor(status), message: apiError.message, details };
}

function titleFor(status: number): string {
  switch (status) {
    case 400:
      return 'Please check your input';
    case 401:
      return 'Sign-in failed'; // protected calls are handled separately as "Session expired"
    case 404:
      return 'Not found';
    case 409:
      return 'Conflict';
    default:
      return status >= 500 ? 'Something went wrong' : 'Request failed';
  }
}

function isApiResult(value: unknown): value is ApiResult {
  return typeof value === 'object' && value !== null && 'isSuccess' in value;
}

// FileReader instead of Blob.text(): it is what NSwag uses and it works in every test DOM.
function readBlobText(blob: Blob): Promise<string> {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = () => resolve(String(reader.result ?? ''));
    reader.onerror = () => reject(reader.error);
    reader.readAsText(blob);
  });
}

/** Extracts the envelope error code from what a generated client throws (parsed `ApiResult` or `ApiException`). */
export function apiErrorCode(thrown: unknown): string | undefined {
  if (isApiResult(thrown)) {
    return thrown.error?.code;
  }

  return undefined;
}
