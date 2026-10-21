import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

import { AuthStore } from './auth.store';

/** Protected pages: unauthenticated visitors go to /login and come back afterwards. */
export const authGuard: CanActivateFn = (_route, state) =>
  inject(AuthStore).hasValidSession()
    ? true
    : inject(Router).createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });

/** Login/register: already signed-in users go straight to their board. */
export const guestGuard: CanActivateFn = () =>
  inject(AuthStore).hasValidSession() ? inject(Router).createUrlTree(['/tasks']) : true;

/** Only same-app relative paths are accepted as a post-login destination (prevents open redirects). */
export function safeReturnUrl(url: string | null | undefined): string {
  return url && url.startsWith('/') && !url.startsWith('//') && !url.startsWith('/\\') ? url : '/tasks';
}
