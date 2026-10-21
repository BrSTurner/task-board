import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { provideToastr } from 'ngx-toastr';

import { routes } from './app.routes';
import { API_BASE_URL } from './core/api/generated/api-client';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes, withComponentInputBinding()),
    provideHttpClient(withInterceptors([])),
    provideToastr({ positionClass: 'toast-bottom-right', timeOut: 3000, preventDuplicates: true }),
    // Same-origin: the Angular dev proxy forwards /api to the backend (see proxy.conf.json).
    { provide: API_BASE_URL, useValue: '' },
  ],
};
