import { Component, inject, ChangeDetectionStrategy } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { BrandComponent } from '../../../shared/components/brand/brand';

@Component({
  selector: 'app-forgot-password',
  imports: [ReactiveFormsModule, RouterLink, BrandComponent],
  templateUrl: './forgot-password.html',
  styleUrl: './forgot-password.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ForgotPasswordComponent {
  private http = inject(HttpClient);

  protected sent = false;
  protected loading = false;
  protected resetUrl = '';

  protected form = new FormGroup({
    email: new FormControl('', [Validators.required, Validators.email]),
  });

  protected onSubmit(): void {
    if (this.form.invalid) return;
    this.loading = true;

    this.http.post<{ message: string; resetUrl?: string }>(
      `${environment.apiUrl}/auth/forgot-password`, { email: this.form.value.email },
    ).subscribe({
      next: res => {
        this.sent = true;
        this.loading = false;
        if (res.resetUrl) this.resetUrl = res.resetUrl;
      },
      error: () => {
        this.sent = true;
        this.loading = false;
      },
    });
  }
}