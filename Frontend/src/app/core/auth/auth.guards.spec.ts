import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, Router, RouterStateSnapshot, UrlTree, provideRouter } from '@angular/router';

import { authGuard, guestGuard, safeReturnUrl } from './auth.guards';
import { AuthStore } from './auth.store';

describe('auth guards', () => {
  let signedIn = false;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideRouter([]), { provide: AuthStore, useValue: { hasValidSession: () => signedIn } }],
    });
  });

  const run = (guard: typeof authGuard, url = '/tasks') =>
    TestBed.runInInjectionContext(() => guard({} as ActivatedRouteSnapshot, { url } as RouterStateSnapshot));

  it('sends signed-out visitors of the board to login with a return URL (BR-001)', () => {
    signedIn = false;

    const result = run(authGuard) as UrlTree;

    expect(TestBed.inject(Router).serializeUrl(result)).toBe('/login?returnUrl=%2Ftasks');
  });

  it('lets signed-in users reach the board and keeps them away from login/register', () => {
    signedIn = true;

    expect(run(authGuard)).toBe(true);
    expect(TestBed.inject(Router).serializeUrl(run(guestGuard) as UrlTree)).toBe('/tasks');
  });

  it('only accepts same-app relative return URLs', () => {
    expect(safeReturnUrl('/tasks')).toBe('/tasks');
    expect(safeReturnUrl('https://evil.example')).toBe('/tasks');
    expect(safeReturnUrl('//evil.example')).toBe('/tasks');
    expect(safeReturnUrl(undefined)).toBe('/tasks');
  });
});
