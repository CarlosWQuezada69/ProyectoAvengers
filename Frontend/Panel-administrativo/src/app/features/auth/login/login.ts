import { Component, inject } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class LoginComponent {
  private authService = inject(AuthService);
  private router = inject(Router);

  protected loading = false;
  protected error = '';

  protected form = new FormGroup({
    email: new FormControl('', [Validators.required, Validators.email]),
    password: new FormControl('', [Validators.required]),
  });

  protected onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.error = '';
    const { email, password } = this.form.value;

    this.authService.login(email!, password!).subscribe({
      next: (res) => {
        this.authService.setTokens(res.accessToken, res.refreshToken);
        this.authService.setUser(res.user);
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.loading = false;
        this.error = err.error?.title || 'Credenciales inválidas';
      },
    });
  }
}
