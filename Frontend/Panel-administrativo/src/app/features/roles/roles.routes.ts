import { Routes } from '@angular/router';

export const roleRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./role-list/role-list').then(m => m.RoleListComponent),
  },
  {
    path: ':id',
    loadComponent: () => import('./role-permissions/role-permissions').then(m => m.RolePermissionsComponent),
  },
];
