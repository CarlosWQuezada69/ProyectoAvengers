import { Routes } from '@angular/router';

export const errorRoutes: Routes = [
  {
    path: ':code',
    loadComponent: () => import('./error-page/error-page').then(m => m.ErrorPageComponent),
  },
  { path: '**', redirectTo: '/error/404' },
];
