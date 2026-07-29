import { Routes } from '@angular/router';

export const settingsRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./settings-form/settings-form').then(m => m.SettingsFormComponent),
  },
];
