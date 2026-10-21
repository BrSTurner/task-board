import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';

import { authInterceptor } from './auth.interceptor';
import { AuthStore } from './auth.store';

describe('authInterceptor', () => {
  const token = signal<string | null>(null);
  let http: HttpClient;
  let backend: HttpTestingController;

  beforeEach(() => {
    token.set(null);
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        { provide: AuthStore, useValue: { accessToken: token } },
      ],
    });
    http = TestBed.inject(HttpClient);
    backend = TestBed.inject(HttpTestingController);
  });

  afterEach(() => backend.verify());

  it('adds the bearer token to API requests when signed in', () => {
    token.set('jwt');

    http.get('/api/tasks').subscribe();

    expect(backend.expectOne('/api/tasks').request.headers.get('Authorization')).toBe('Bearer jwt');
  });

  it('sends no Authorization header when signed out or for non-API URLs', () => {
    http.get('/api/tasks').subscribe();
    token.set('jwt');
    http.get('https://example.test/other').subscribe();

    expect(backend.expectOne('/api/tasks').request.headers.has('Authorization')).toBe(false);
    expect(backend.expectOne('https://example.test/other').request.headers.has('Authorization')).toBe(false);
  });
});
