import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, from, switchMap, throwError } from 'rxjs';

import { AuthStore } from '../auth/auth.store';
import { readApiError, toDisplayError } from './api-error';
import { ErrorDialogService } from './error-dialog.service';

const LOGIN_URL = '/api/auth/login';

/**
 * Single place where API failures become UI: every error is shown in a modal (stores only roll back state).
 * A 401 on anything except the login request means the session is gone, so the user is signed out.
 */
export const errorInterceptor: HttpInterceptorFn = (request, next) => {
  const errors = inject(ErrorDialogService);
  const auth = inject(AuthStore);
  const router = inject(Router);

  return next(request).pipe(
    catchError((failure: unknown) => {
      if (!(failure instanceof HttpErrorResponse)) {
        return throwError(() => failure);
      }

      return from(readApiError(failure)).pipe(
        switchMap((apiError) => {
          if (failure.status === 401 && !request.url.endsWith(LOGIN_URL)) {
            const returnUrl = router.url;
            auth.clearSession();
            void router.navigate(['/login'], { queryParams: returnUrl.startsWith('/tasks') ? { returnUrl } : {} });
            errors.show({
              title: 'Session expired',
              message: apiError?.message && apiError.code !== 'Unauthorized'
                ? apiError.message
                : 'Your session has expired. Please sign in again.',
              details: [],
            });
          } else {
            errors.show(toDisplayError(failure.status, apiError));
          }

          return throwError(() => failure);
        }),
      );
    }),
  );
};
