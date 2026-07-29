import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import type { User } from '../models/user';

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  user: User;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);

  private readonly tokenKey = 'access_token';
  private readonly refreshKey = 'refresh_token';

  private userSignal = signal<User | null>(null);
  private authenticatedSignal = signal(false);

  readonly user = this.userSignal.asReadonly();
  readonly isAuthenticatedSignal = this.authenticatedSignal.asReadonly();
  readonly permissions = computed(() => this.userSignal()?.permissions ?? []);

  isAuthenticated(): boolean {
    return !!localStorage.getItem(this.tokenKey);
  }

  hasPermission(permission: string): boolean {
    return this.permissions().includes(permission);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(this.refreshKey);
  }

  setTokens(accessToken: string, refreshToken: string): void {
    localStorage.setItem(this.tokenKey, accessToken);
    localStorage.setItem(this.refreshKey, refreshToken);
  }

  setUser(user: User): void {
    this.userSignal.set(user);
    this.authenticatedSignal.set(true);
  }

  clearTokens(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.refreshKey);
  }

  login(email: string, password: string) {
    return this.http.post<LoginResponse>(`${environment.apiUrl}/auth/login`, { email, password });
  }

  refreshToken() {
    const refresh = this.getRefreshToken();
    return this.http.post<LoginResponse>(`${environment.apiUrl}/auth/refresh-token`, { refreshToken: refresh });
  }

  logout() {
    return this.http.post(`${environment.apiUrl}/auth/logout`, {});
  }

  me() {
    return this.http.get<User>(`${environment.apiUrl}/auth/me`);
  }

  loadUser(): void {
    this.me().subscribe({
      next: (user) => {
        this.userSignal.set(user);
        this.authenticatedSignal.set(true);
      },
      error: () => {
        this.clearTokens();
        this.userSignal.set(null);
        this.authenticatedSignal.set(false);
        this.router.navigate(['/auth/login']);
      },
    });
  }
}
