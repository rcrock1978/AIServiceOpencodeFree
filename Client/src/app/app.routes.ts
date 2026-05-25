import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: '/store', pathMatch: 'full' },
  {
    path: 'auth',
    loadChildren: () => import('./features/auth/auth.routes').then(m => m.authRoutes)
  },
  {
    path: 'store',
    loadChildren: () => import('./features/store/store.routes').then(m => m.storeRoutes)
  },
  { path: '**', redirectTo: '/store' }
];
