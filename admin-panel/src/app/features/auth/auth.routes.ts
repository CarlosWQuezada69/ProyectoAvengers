import { Routes } from '@angular/router';

export const authRoutes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./login/login').then(m => m.LoginComponent),
  },
  {
    path: 'forgot-password',
    loadComponent: () => import('./forgot-password/forgot-password').then(m => m.ForgotPasswordComponent),
  },
  {
    path: 'reset-password',
    loadComponent: () => import('./reset-password/reset-password').then(m => m.ResetPasswordComponent),
  },
  {
    path: 'confirm-email',
    loadComponent: () => import('./confirm-email/confirm-email').then(m => m.ConfirmEmailComponent),
  },
  { path: '**', redirectTo: 'login' },
];
