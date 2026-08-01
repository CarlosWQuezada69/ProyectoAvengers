import { Component, inject, ChangeDetectionStrategy } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { BrandComponent } from '../../../shared/components/brand/brand';

interface ErrorConfig {
  code: string;
  title: string;
  message: string;
  icon: string;
}

const ERROR_MAP: Record<string, ErrorConfig> = {
  '404': {
    code: '404',
    title: 'Página no encontrada',
    message: 'La página que buscas no existe o ha sido movida.',
    icon: 'M12 9v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z',
  },
  '403': {
    code: '403',
    title: 'Acceso denegado',
    message: 'No tienes permisos para acceder a esta página.',
    icon: 'M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z',
  },
  '500': {
    code: '500',
    title: 'Error del servidor',
    message: 'Ocurrió un error inesperado. Intenta de nuevo más tarde.',
    icon: 'M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z',
  },
};

@Component({
  selector: 'app-error-page',
  imports: [RouterLink, BrandComponent],
  templateUrl: './error-page.html',
  styleUrl: './error-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ErrorPageComponent {
  private route = inject(ActivatedRoute);

  get config(): ErrorConfig {
    const code = this.route.snapshot.paramMap.get('code') ?? '404';
    return ERROR_MAP[code] ?? ERROR_MAP['404'];
  }

  goBack(): void {
    window.history.back();
  }
}
