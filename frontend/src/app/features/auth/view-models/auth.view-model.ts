import { HttpErrorResponse } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, finalize, map, of, shareReplay, switchMap, tap, throwError } from 'rxjs';

import {
  ACCESS_TOKEN_EXPIRES_AT_KEY,
  ACCESS_TOKEN_KEY,
  REFRESH_TOKEN_KEY,
  USER_KEY
} from '../../../core/auth/auth-session.constants';
import { AuthApiService } from '../data/auth-api.service';
import { AuthResponse, LoginRequest, RegisterRequest, UserProfile } from '../models/auth.models';

@Injectable({ providedIn: 'root' })
export class AuthViewModel {
  private readonly authApi = inject(AuthApiService);
  private readonly router = inject(Router);

  private readonly accessToken = signal<string | null>(localStorage.getItem(ACCESS_TOKEN_KEY));
  private readonly currentUser = signal<UserProfile | null>(this.readStoredUser());
  private readonly loading = signal(false);
  private readonly error = signal<string | null>(null);
  private readonly success = signal<string | null>(null);

  private refreshRequest$?: ReturnType<typeof this.authApi.refresh>;
  private refreshTimer: ReturnType<typeof setTimeout> | null = null;

  readonly isLoading = this.loading.asReadonly();
  readonly errorMessage = this.error.asReadonly();
  readonly successMessage = this.success.asReadonly();
  readonly user = this.currentUser.asReadonly();
  readonly isAuthenticated = computed(() => Boolean(this.accessToken()));

  constructor() {
    this.scheduleRefreshFromStorage();
    void this.restoreSession();
  }

  login(payload: LoginRequest) {
    this.loading.set(true);
    this.error.set(null);
    this.success.set(null);

    this.authApi
      .login(payload)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (response) => {
          this.persistSession(response);
          this.success.set(`Bine ai revenit, ${this.resolveDisplayName(response.user)}.`);
          void this.router.navigate(['/dashboard']);
        },
        error: (error: unknown) => {
          this.error.set(this.extractErrorMessage(error, 'Nu am putut face autentificarea.'));
        }
      });
  }

  register(payload: RegisterRequest) {
    this.loading.set(true);
    this.error.set(null);
    this.success.set(null);

    this.authApi
      .register(payload)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (response) => {
          this.persistSession(response);
          this.success.set('Contul a fost creat cu succes.');
          void this.router.navigate(['/dashboard']);
        },
        error: (error: unknown) => {
          this.error.set(this.extractErrorMessage(error, 'Nu am putut crea contul.'));
        }
      });
  }

  refreshSession() {
    const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY);
    if (!refreshToken) {
      return throwError(() => new Error('Missing refresh token.'));
    }

    if (!this.refreshRequest$) {
      this.refreshRequest$ = this.authApi.refresh({ refreshToken }).pipe(
        tap((response) => this.persistSession(response)),
        shareReplay(1)
      );
    }

    return this.refreshRequest$.pipe(
      map((response) => response.accessToken),
      catchError((error) => {
        this.clearSession();
        return throwError(() => error);
      }),
      finalize(() => {
        this.refreshRequest$ = undefined;
      })
    );
  }

  logout() {
    const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY);

    if (!refreshToken) {
      this.logoutLocally();
      return;
    }

    this.authApi
      .logout(refreshToken)
      .pipe(
        catchError(() => of(void 0)),
        finalize(() => {
          this.logoutLocally();
        })
      )
      .subscribe();
  }

  logoutLocally() {
    this.clearSession();
    void this.router.navigate(['/auth/login']);
  }

  currentAccessToken() {
    return this.accessToken();
  }

  hasRefreshToken() {
    return Boolean(localStorage.getItem(REFRESH_TOKEN_KEY));
  }

  private async restoreSession() {
    const token = this.accessToken();
    if (!token) {
      return;
    }

    try {
      const user = await this.authApi
        .me()
        .pipe(
          catchError((error) => {
            if (error instanceof HttpErrorResponse && error.status === 401 && this.hasRefreshToken()) {
              return this.refreshSession().pipe(switchMap(() => this.authApi.me()));
            }

            return throwError(() => error);
          })
        )
        .toPromise();

      if (user) {
        this.storeUser(user);
      }
    } catch {
      this.clearSession();
    }
  }

  private persistSession(response: AuthResponse) {
    this.accessToken.set(response.accessToken);
    this.storeUser(response.user);

    localStorage.setItem(ACCESS_TOKEN_KEY, response.accessToken);
    localStorage.setItem(REFRESH_TOKEN_KEY, response.refreshToken);
    localStorage.setItem(ACCESS_TOKEN_EXPIRES_AT_KEY, response.accessTokenExpiresAt);

    this.scheduleRefresh(response.accessTokenExpiresAt);
  }

  private storeUser(user: UserProfile) {
    const normalizedUser = this.normalizeUser(user);
    this.currentUser.set(normalizedUser);
    localStorage.setItem(USER_KEY, JSON.stringify(normalizedUser));
  }

  private readStoredUser(): UserProfile | null {
    const rawValue = localStorage.getItem(USER_KEY);
    if (!rawValue) {
      return null;
    }

    try {
      return this.normalizeUser(JSON.parse(rawValue) as UserProfile);
    } catch {
      localStorage.removeItem(USER_KEY);
      return null;
    }
  }

  private normalizeUser(user: UserProfile | null): UserProfile | null {
    if (!user) {
      return null;
    }

    const fullName = user.fullName || [user.firstName, user.lastName].filter(Boolean).join(' ').trim();

    return {
      ...user,
      id: user.id || user.userId,
      fullName: fullName || user.name || undefined,
      roles: user.roles ?? (user.role ? [user.role] : [])
    };
  }

  private clearSession() {
    this.accessToken.set(null);
    this.currentUser.set(null);
    this.error.set(null);
    this.success.set(null);
    if (this.refreshTimer) {
      clearTimeout(this.refreshTimer);
      this.refreshTimer = null;
    }
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(ACCESS_TOKEN_EXPIRES_AT_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
  }

  private resolveDisplayName(user: UserProfile | null): string {
    if (!user) {
      return 'utilizator';
    }

    const name = [user.firstName, user.lastName].filter(Boolean).join(' ').trim();
    return user.fullName || user.name || name || user.email || 'utilizator';
  }

  private scheduleRefreshFromStorage() {
    const expiresAt = localStorage.getItem(ACCESS_TOKEN_EXPIRES_AT_KEY);
    if (expiresAt) {
      this.scheduleRefresh(expiresAt);
    }
  }

  private scheduleRefresh(expiresAt: string) {
    if (this.refreshTimer) {
      clearTimeout(this.refreshTimer);
      this.refreshTimer = null;
    }

    const refreshAt = new Date(expiresAt).getTime() - Date.now() - 60_000;
    if (refreshAt <= 0) {
      return;
    }

    this.refreshTimer = setTimeout(() => {
      this.refreshSession()
        .pipe(
          catchError(() => {
            this.logoutLocally();
            return of(null);
          })
        )
        .subscribe();
    }, refreshAt);
  }

  private extractErrorMessage(error: unknown, fallback: string): string {
    if (error instanceof HttpErrorResponse) {
      const candidate = error.error;

      if (typeof candidate === 'string' && candidate.trim()) {
        return candidate;
      }

      if (candidate && typeof candidate === 'object') {
        const messageCandidate = (candidate as { message?: unknown }).message;
        if (typeof messageCandidate === 'string' && messageCandidate.trim()) {
          return messageCandidate;
        }

        const titleCandidate = (candidate as { title?: unknown }).title;
        if (typeof titleCandidate === 'string' && titleCandidate.trim()) {
          return titleCandidate;
        }

        const errorsCandidate = (candidate as { errors?: Record<string, string[] | string> }).errors;
        if (errorsCandidate && typeof errorsCandidate === 'object') {
          const firstError = Object.values(errorsCandidate)
            .flatMap((entry) => (Array.isArray(entry) ? entry : [entry]))
            .find((entry) => typeof entry === 'string' && entry.trim());

          if (typeof firstError === 'string') {
            return firstError;
          }
        }
      }
    }

    return fallback;
  }
}
