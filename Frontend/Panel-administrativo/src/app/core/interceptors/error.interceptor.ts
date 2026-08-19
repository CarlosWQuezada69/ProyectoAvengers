import { inject } from '@angular/core';
import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';
import { ToastService } from '../../shared/services/toast.service';
import type { ProblemDetails } from '../models/problem-details';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const toast = inject(ToastService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      const problem: ProblemDetails | undefined = error.error;
      const details = problem?.errors
        ? Object.values(problem.errors).flat().join(' ')
        : problem?.detail;

      switch (error.status) {
        case 400:
          toast.error(details ?? problem?.title ?? 'Solicitud inválida');
          break;
        case 401:
          toast.error('Sesión expirada o credenciales inválidas. Inicia sesión de nuevo.');
          break;
        case 403:
          toast.error('No tienes permiso para realizar esta acción');
          break;
        case 404:
          toast.error('Recurso no encontrado');
          break;
        case 409:
          toast.error(details ?? problem?.title ?? 'Conflicto al procesar la solicitud');
          break;
        case 429:
          toast.warning('Demasiadas solicitudes. Intenta de nuevo más tarde');
          break;
        case 500:
          toast.error('Error interno del servidor');
          break;
      }

      return throwError(() => error);
    })
  );
};
