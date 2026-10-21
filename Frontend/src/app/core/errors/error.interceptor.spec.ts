import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';

import { AuthStore } from '../auth/auth.store';
import { ErrorDialogService } from './error-dialog.service';
import { errorInterceptor } from './error.interceptor';

/** NSwag clients use responseType "blob", so the interceptor must read envelopes from Blob bodies. */
function envelopeBlob(code: string, message: string, validationErrors?: Record<string, string[]>): Blob {
  return new Blob([JSON.stringify({ isSuccess: false, error: { code, message, validationErrors } })], { type: 'application/json' });
}

describe('errorInterceptor', () => {
  let http: HttpClient;
  let backend: HttpTestingController;
  let errors: { show: ReturnType<typeof vi.fn> };
  let auth: { clearSession: ReturnType<typeof vi.fn> };
  let router: { url: string; navigate: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    errors = { show: vi.fn() };
    auth = { clearSession: vi.fn() };
    router = { url: '/tasks', navigate: vi.fn().mockResolvedValue(true) };
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([errorInterceptor])),
        provideHttpClientTesting(),
        { provide: ErrorDialogService, useValue: errors },
        { provide: AuthStore, useValue: auth },
        { provide: Router, useValue: router },
      ],
    });
    http = TestBed.inject(HttpClient);
    backend = TestBed.inject(HttpTestingController);
  });

  afterEach(() => backend.verify());

  async function fail(url: string, status: number, body: Blob | null): Promise<void> {
    const request = firstValueFrom(http.get(url, { responseType: 'blob' }));
    backend.expectOne(url).flush(body, { status, statusText: 'Error' });
    await expect(request).rejects.toBeTruthy(); // still rethrown so stores can roll back
  }

  it('shows the API message and validation details in the error modal', async () => {
    await fail('/api/tasks', 400, envelopeBlob('Validation', 'One or more validation errors occurred.', {
      title: ['Title is required'],
      dueDate: ['Due date is required'],
    }));

    expect(errors.show).toHaveBeenCalledWith({
      title: 'Please check your input',
      message: 'One or more validation errors occurred.',
      details: ['Title is required', 'Due date is required'],
    });
  });

  it('shows the conflict message for a duplicate registration (Scenario 3)', async () => {
    await fail('/api/auth/register', 409, envelopeBlob('Auth.EmailTaken', 'An account with this email already exists.'));

    expect(errors.show).toHaveBeenCalledWith(expect.objectContaining({ message: 'An account with this email already exists.' }));
  });

  it('signs out and redirects to login when a protected call returns 401', async () => {
    await fail('/api/tasks', 401, envelopeBlob('Unauthorized', 'Authentication is required. Please sign in.'));

    expect(auth.clearSession).toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/login'], { queryParams: { returnUrl: '/tasks' } });
    expect(errors.show).toHaveBeenCalledWith(expect.objectContaining({ title: 'Session expired' }));
  });

  it('treats 401 from the login request as invalid credentials, without redirect loops', async () => {
    await fail('/api/auth/login', 401, envelopeBlob('Auth.InvalidCredentials', 'Invalid email or password.'));

    expect(auth.clearSession).not.toHaveBeenCalled();
    expect(router.navigate).not.toHaveBeenCalled();
    expect(errors.show).toHaveBeenCalledWith(expect.objectContaining({ message: 'Invalid email or password.' }));
  });

  it('explains that the server is unreachable for network errors and non-JSON proxy failures', async () => {
    const request = firstValueFrom(http.get('/api/tasks'));
    backend.expectOne('/api/tasks').error(new ProgressEvent('error'), { status: 0 });
    await expect(request).rejects.toBeTruthy();

    await fail('/api/tasks', 504, new Blob(['<html>Gateway Timeout</html>'], { type: 'text/html' }));

    expect(errors.show).toHaveBeenCalledTimes(2);
    expect(errors.show.mock.calls.every(([e]) => e.title === 'Server unavailable')).toBe(true);
  });
});
