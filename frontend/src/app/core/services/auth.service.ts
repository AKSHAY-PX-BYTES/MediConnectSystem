import { Injectable, computed, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AppUser,
  AuthResponse,
  LoginRequest,
  RegisterClinicRequest
} from '../models/auth.models';

const ACCESS_TOKEN_KEY = 'mc_access_token';
const REFRESH_TOKEN_KEY = 'mc_refresh_token';
const USER_KEY = 'mc_user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly baseUrl = `${environment.apiUrl}/auth`;

  private readonly _user = signal<AppUser | null>(this.readUser());
  readonly user = this._user.asReadonly();
  readonly isAuthenticated = computed(() => this._user() !== null);
  readonly role = computed(() => this._user()?.role ?? null);

  constructor(private readonly http: HttpClient) {}

  login(payload: LoginRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.baseUrl}/login`, payload)
      .pipe(tap((res) => this.persist(res)));
  }

  registerClinic(payload: RegisterClinicRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.baseUrl}/register-clinic`, payload)
      .pipe(tap((res) => this.persist(res)));
  }

  refresh(): Observable<AuthResponse> {
    const refreshToken = this.getRefreshToken();
    return this.http
      .post<AuthResponse>(`${this.baseUrl}/refresh`, { refreshToken })
      .pipe(tap((res) => this.persist(res)));
  }

  logout(): void {
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    this._user.set(null);
  }

  getAccessToken(): string | null {
    return localStorage.getItem(ACCESS_TOKEN_KEY);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(REFRESH_TOKEN_KEY);
  }

  private persist(res: AuthResponse): void {
    localStorage.setItem(ACCESS_TOKEN_KEY, res.accessToken);
    localStorage.setItem(REFRESH_TOKEN_KEY, res.refreshToken);
    localStorage.setItem(USER_KEY, JSON.stringify(res.user));
    this._user.set(res.user);
  }

  private readUser(): AppUser | null {
    const raw = localStorage.getItem(USER_KEY);
    return raw ? (JSON.parse(raw) as AppUser) : null;
  }
}
