import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { jwtDecode } from 'jwt-decode';
import { AuthResponse, User } from '../core/models/user.model';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);

  private tokenSignal = signal<string | null>(localStorage.getItem('auth_token'));
  private userSignal = signal<User | null>(this.loadUser());

  readonly currentUser = this.userSignal.asReadonly();
  readonly isLoggedIn = computed(() => {
    const token = this.tokenSignal();
    if (!token) return false;
    try {
      const decoded: any = jwtDecode(token);
      return !(decoded.exp && decoded.exp * 1000 < Date.now());
    } catch {
      return false;
    }
  });

  constructor() {
    if (this.tokenSignal() && !this.isLoggedIn()) {
      this.clearAuth();
    }
  }

  login(email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>('/api/auth/login', { email, password }).pipe(
      tap(res => this.setAuth(res.token, res.user))
    );
  }

  register(email: string, username: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>('/api/auth/register', { email, username, password }).pipe(
      tap(res => this.setAuth(res.token, res.user))
    );
  }

  logout(): void {
    this.clearAuth();
    this.router.navigate(['/auth/login']);
  }

  getToken(): string | null {
    return this.tokenSignal();
  }

  private setAuth(token: string, user?: User): void {
    localStorage.setItem('auth_token', token);
    if (user) localStorage.setItem('auth_user', JSON.stringify(user));
    this.tokenSignal.set(token);
    this.userSignal.set(user ?? null);
  }

  private clearAuth(): void {
    localStorage.removeItem('auth_token');
    localStorage.removeItem('auth_user');
    this.tokenSignal.set(null);
    this.userSignal.set(null);
  }

  private loadUser(): User | null {
    try {
      const stored = localStorage.getItem('auth_user');
      return stored ? JSON.parse(stored) : null;
    } catch {
      return null;
    }
  }
}
