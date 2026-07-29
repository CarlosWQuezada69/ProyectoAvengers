import { inject } from '@angular/core';
import { Router, type CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const permissionGuard: CanActivateFn = (route) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const requiredPermission = route.data?.['permission'] as string | undefined;

  if (!requiredPermission) return true;

  if (authService.hasPermission(requiredPermission)) {
    return true;
  }

  return router.parseUrl('/dashboard');
};
