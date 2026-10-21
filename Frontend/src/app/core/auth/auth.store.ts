import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';

import { AuthClient, LoginResponse, RegisterUserRequest, UserDto } from '../api/generated/api-client';

const STORAGE_KEY = 'taskboard.session';

interface Session {
  accessToken: string;
  /** ISO UTC instant from the API. */
  expiresAt: string;
  user: UserDto;
}

/**
 * Authentication state. The access token lives in sessionStorage (cleared when the tab closes). This is a
 * demo trade-off: JS-readable storage is exposed to XSS — see README for the production alternative.
 */
@Injectable({ providedIn: 'root' })
export class AuthStore {
  private readonly authClient = inject(AuthClient);
  private readonly router = inject(Router);

  private readonly session = signal<Session | null>(readStoredSession());

  readonly user = computed(() => this.session()?.user ?? null);
  readonly accessToken = computed(() => this.session()?.accessToken ?? null);
  readonly isAuthenticated = computed(() => this.session() !== null);

  /** Checked by guards: an expired token is treated as signed out (and cleared). */
  hasValidSession(): boolean {
    const session = this.session();
    if (session && isExpired(session)) {
      this.clearSession();
      return false;
    }

    return session !== null;
  }

  async login(email: string, password: string): Promise<UserDto> {
    const result = await firstValueFrom(this.authClient.login({ email, password }));
    const data = result.data as LoginResponse;
    const session: Session = { accessToken: data.accessToken, expiresAt: data.expiresAt, user: data.user };
    sessionStorage.setItem(STORAGE_KEY, JSON.stringify(session));
    this.session.set(session);
    return data.user;
  }

  /** Registration does not sign the user in; they are sent to the login page afterwards. */
  async register(request: RegisterUserRequest): Promise<UserDto> {
    const result = await firstValueFrom(this.authClient.register(request));
    return result.data as UserDto;
  }

  logout(): void {
    this.clearSession();
    void this.router.navigate(['/login']);
  }

  clearSession(): void {
    sessionStorage.removeItem(STORAGE_KEY);
    this.session.set(null);
  }
}

function readStoredSession(): Session | null {
  try {
    const raw = sessionStorage.getItem(STORAGE_KEY);
    const session = raw ? (JSON.parse(raw) as Session) : null;
    if (!session?.accessToken || !session.user || isExpired(session)) {
      sessionStorage.removeItem(STORAGE_KEY);
      return null;
    }

    return session;
  } catch {
    sessionStorage.removeItem(STORAGE_KEY);
    return null;
  }
}

function isExpired(session: Session): boolean {
  const expiresAt = Date.parse(session.expiresAt);
  return Number.isNaN(expiresAt) || expiresAt <= Date.now();
}
