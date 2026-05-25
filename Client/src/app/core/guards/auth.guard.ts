import { inject } from '@angular/core';
import { Router, type UrlTree } from '@angular/router';
import { jwtDecode } from 'jwt-decode';

export const authGuard = (): boolean | UrlTree => {
  const router = inject(Router);
  const token = localStorage.getItem('auth_token');
  if (!token) return router.parseUrl('/auth/login');
  try {
    const decoded: any = jwtDecode(token);
    if (decoded.exp && decoded.exp * 1000 < Date.now()) {
      localStorage.removeItem('auth_token');
      localStorage.removeItem('auth_user');
      return router.parseUrl('/auth/login');
    }
    return true;
  } catch {
    return router.parseUrl('/auth/login');
  }
};
