import { Component, inject, ChangeDetectionStrategy } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { ToastService } from '../../../shared/services/toast.service';
import { BrandComponent } from '../../../shared/components/brand/brand';

@Component({
  selector: 'app-reset-password',
  imports: [ReactiveFormsModule, RouterLink, BrandComponent],
  templateUrl: './reset-password.html',
  styleUrl: './reset-password.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResetPasswordComponent {
  private http = inject(HttpClient);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private toast = inject(ToastService);

  protected loading = false;
  protected token = this.route.snapshot.queryParamMap.get('token') ?? '';

  protected form = new FormGroup({
    password: new FormControl('', [Validators.required, Validators.minLength(8)]),
    confirmPassword: new FormControl('', [Validators.required]),
  });

  protected onSubmit(): void {
    if (this.form.invalid || this.form.value.password !== this.form.value.confirmPassword) return;
    this.loading = true;

    this.http.post(`${environment.apiUrl}/auth/reset-password`, {
      token: this.token,
      password: this.form.value.password,
    }).subscribe({
      next: () => {
        this.toast.show('Contraseña restablecida correctamente', 'success');
        this.router.navigate(['/auth/login']);
      },
      error: () => {
        this.loading = false;
        this.toast.show('El enlace ha expirado o es inválido', 'error');
      },
    });
  }
}
