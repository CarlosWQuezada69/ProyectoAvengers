import { Routes } from '@angular/router';

export const auditRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./audit-log/audit-log').then(m => m.AuditLogComponent),
  },
];
