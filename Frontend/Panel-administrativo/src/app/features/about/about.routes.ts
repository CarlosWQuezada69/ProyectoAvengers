import { Routes } from '@angular/router';

export const aboutRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./about-form/about-form').then(m => m.AboutFormComponent),
  },
];