import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { permissionGuard } from './core/guards/permission.guard';

export const routes: Routes = [
  {
    path: 'auth',
    loadChildren: () => import('./features/auth/auth.routes').then(m => m.authRoutes),
  },
  {
    path: 'error',
    loadChildren: () => import('./features/errors/error.routes').then(m => m.errorRoutes),
  },
  {
    path: '',
    loadComponent: () => import('./shared/layout/layout').then(m => m.LayoutComponent),
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadChildren: () => import('./features/dashboard/dashboard.routes').then(m => m.dashboardRoutes),
      },
      {
        path: 'products',
        loadChildren: () => import('./features/products/products.routes').then(m => m.productRoutes),
        canActivate: [permissionGuard],
        data: { permission: 'products.view' },
      },
      {
        path: 'categories',
        loadChildren: () => import('./features/categories/categories.routes').then(m => m.categoryRoutes),
        canActivate: [permissionGuard],
        data: { permission: 'categories.view' },
      },
      {
        path: 'slider',
        loadChildren: () => import('./features/slider/slider.routes').then(m => m.sliderRoutes),
        canActivate: [permissionGuard],
        data: { permission: 'slider.view' },
      },
      {
        path: 'settings',
        loadChildren: () => import('./features/settings/settings.routes').then(m => m.settingsRoutes),
        canActivate: [permissionGuard],
        data: { permission: 'settings.view' },
      },
      {
        path: 'users',
        loadChildren: () => import('./features/users/users.routes').then(m => m.userRoutes),
        canActivate: [permissionGuard],
        data: { permission: 'users.view' },
      },
      {
        path: 'roles',
        loadChildren: () => import('./features/roles/roles.routes').then(m => m.roleRoutes),
        canActivate: [permissionGuard],
        data: { permission: 'roles.view' },
      },
      {
        path: 'audit',
        loadChildren: () => import('./features/audit/audit.routes').then(m => m.auditRoutes),
        canActivate: [permissionGuard],
        data: { permission: 'audit.view' },
      },
      {
        path: 'profile',
        loadComponent: () => import('./features/profile/profile').then(m => m.ProfileComponent),
      },
      {
        path: 'change-password',
        loadComponent: () => import('./features/change-password/change-password').then(m => m.ChangePasswordComponent),
      },
      { path: '**', redirectTo: '/error/404' },
    ],
  },
  { path: '**', redirectTo: '/error/404' },
];
