import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';

import { apiConfig } from '../../../core/config/api.config';
import {
  AuthResponse,
  ChangePasswordRequest,
  LoginRequest,
  RefreshTokenRequest,
  RegisterRequest,
  UserProfile
} from '../models/auth.models';

@Injectable({ providedIn: 'root' })
export class AuthApiService {
  private readonly http = inject(HttpClient);
  private readonly authUrl = `${apiConfig.gatewayBaseUrl}${apiConfig.authBasePath}`;

  login(payload: LoginRequest) {
    return this.http.post<AuthResponse>(`${this.authUrl}/login`, payload);
  }

  register(payload: RegisterRequest) {
    return this.http.post<AuthResponse>(`${this.authUrl}/register`, payload);
  }

  refresh(payload: RefreshTokenRequest) {
    return this.http.post<AuthResponse>(`${this.authUrl}/refresh`, payload);
  }

  me() {
    return this.http.get<UserProfile>(`${this.authUrl}/me`);
  }

  changePassword(payload: ChangePasswordRequest) {
    return this.http.post<void>(`${this.authUrl}/change-password`, payload);
  }

  logout(refreshToken: string) {
    return this.http.post<void>(`${this.authUrl}/logout`, { refreshToken });
  }
}
