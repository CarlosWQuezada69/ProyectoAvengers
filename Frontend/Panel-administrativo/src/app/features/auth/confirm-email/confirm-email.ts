import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { ToastService } from '../../../shared/services/toast.service';
import { BrandComponent } from '../../../shared/components/brand/brand';

@Component({
  selector: 'app-confirm-email',
  imports: [RouterLink, BrandComponent],
  templateUrl: './confirm-email.html',
  styleUrl: './confirm-email.scss',
})
export class ConfirmEmailComponent implements OnInit {
  private http = inject(HttpClient);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private toast = inject(ToastService);

  protected status: 'loading' | 'success' | 'error' = 'loading';
  protected message = 'Confirmando cambio de correo...';

  ngOnInit(): void {
    const token = this.route.snapshot.queryParamMap.get('token') ?? '';

    this.http.get(`${environment.apiUrl}/account/change-email/confirm`, {
      params: { token },
      responseType: 'text',
    }).subscribe({
      next: () => {
        this.status = 'success';
        this.message = 'Correo electrónico confirmado exitosamente';
        setTimeout(() => this.router.navigate(['/auth/login']), 3000);
      },
      error: () => {
        this.status = 'error';
        this.message = 'El enlace ha expirado o es inválido';
      },
    });
  }
}
