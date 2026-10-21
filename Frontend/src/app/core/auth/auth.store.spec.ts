import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';

import { AuthClient } from '../api/generated/api-client';
import { AuthStore } from './auth.store';

const STORAGE_KEY = 'taskboard.session';
const user = { id: 'u1', name: 'Alice', email: 'demo@example.test', timeZone: 'America/Sao_Paulo' };

describe('AuthStore', () => {
  let authClient: { login: ReturnType<typeof vi.fn>; register: ReturnType<typeof vi.fn> };
  let router: { navigate: ReturnType<typeof vi.fn> };

  function createStore(): AuthStore {
    TestBed.configureTestingModule({
      providers: [
        { provide: AuthClient, useValue: authClient },
        { provide: Router, useValue: router },
      ],
    });
    return TestBed.inject(AuthStore);
  }

  beforeEach(() => {
    sessionStorage.clear();
    authClient = { login: vi.fn(), register: vi.fn() };
    router = { navigate: vi.fn().mockResolvedValue(true) };
  });

  it('stores the session after a successful login', async () => {
    const expiresAt = new Date(Date.now() + 60_000).toISOString();
    authClient.login.mockReturnValue(of({ isSuccess: true, data: { accessToken: 'jwt', expiresAt, user } }));
    const store = createStore();

    await store.login('demo@example.test', 'DemoPass123!');

    expect(store.isAuthenticated()).toBe(true);
    expect(store.accessToken()).toBe('jwt');
    expect(store.user()?.timeZone).toBe('America/Sao_Paulo');
    expect(JSON.parse(sessionStorage.getItem(STORAGE_KEY)!).accessToken).toBe('jwt');
  });

  it('stays signed out when the login is rejected (Scenario 5)', async () => {
    authClient.login.mockReturnValue(throwError(() => ({ isSuccess: false, error: { code: 'Auth.InvalidCredentials' } })));
    const store = createStore();

    await expect(store.login('demo@example.test', 'wrong')).rejects.toBeTruthy();

    expect(store.isAuthenticated()).toBe(false);
    expect(sessionStorage.getItem(STORAGE_KEY)).toBeNull();
  });

  it('restores a valid session and discards an expired one', () => {
    sessionStorage.setItem(STORAGE_KEY, JSON.stringify({ accessToken: 'old', expiresAt: '2000-01-01T00:00:00Z', user }));

    const store = createStore();

    expect(store.isAuthenticated()).toBe(false);
    expect(sessionStorage.getItem(STORAGE_KEY)).toBeNull();
  });

  it('ignores corrupted storage', () => {
    sessionStorage.setItem(STORAGE_KEY, '{not json');

    expect(createStore().isAuthenticated()).toBe(false);
  });

  it('logout clears the session and returns to the login page', () => {
    sessionStorage.setItem(
      STORAGE_KEY,
      JSON.stringify({ accessToken: 'jwt', expiresAt: new Date(Date.now() + 60_000).toISOString(), user }),
    );
    const store = createStore();
    expect(store.hasValidSession()).toBe(true);

    store.logout();

    expect(store.isAuthenticated()).toBe(false);
    expect(sessionStorage.getItem(STORAGE_KEY)).toBeNull();
    expect(router.navigate).toHaveBeenCalledWith(['/login']);
  });

  it('register does not sign the user in', async () => {
    authClient.register.mockReturnValue(of({ isSuccess: true, data: user }));
    const store = createStore();

    await store.register({ name: 'Alice', email: 'demo@example.test', timeZone: 'UTC', password: 'DemoPass123!' });

    expect(store.isAuthenticated()).toBe(false);
  });
});
