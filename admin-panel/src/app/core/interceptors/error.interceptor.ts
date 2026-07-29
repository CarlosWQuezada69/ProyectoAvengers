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

      switch (error.status) {
        case 400:
          toast.show(problem?.title ?? 'Solicitud inválida', 'error');
          break;
        case 403:
          toast.show('No tienes permiso para realizar esta acción', 'error');
          break;
        case 404:
          toast.show('Recurso no encontrado', 'error');
          break;
        case 409:
          toast.show(problem?.title ?? 'Conflicto al procesar la solicitud', 'error');
          break;
        case 429:
          toast.show('Demasiadas solicitudes. Intenta de nuevo más tarde', 'warning');
          break;
        case 500:
          toast.show('Error interno del servidor', 'error');
          break;
      }

      return throwError(() => error);
    })
  );
};
