import { Component, inject, ChangeDetectionStrategy } from '@angular/core';
import { AbstractControl, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AccountService } from '../../core/services/account.service';
import { AuthService } from '../../core/services/auth.service';
import { Router } from '@angular/router';
import { ButtonComponent } from '../../shared/components/button/button';
import { InputComponent } from '../../shared/components/input/input';
import { ToastService } from '../../shared/services/toast.service';

@Component({
  selector: 'app-change-password',
  imports: [ReactiveFormsModule, ButtonComponent, InputComponent],
  templateUrl: './change-password.html',
  styleUrl: './change-password.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ChangePasswordComponent {
  private accountService = inject(AccountService);
  private authService = inject(AuthService);
  private router = inject(Router);
  private toast = inject(ToastService);

  protected saving = false;

  protected form = new FormGroup({
    currentPassword: new FormControl('', [Validators.required]),
    newPassword: new FormControl('', [Validators.required, Validators.minLength(8)]),
    confirmPassword: new FormControl('', [Validators.required]),
  }, { validators: this.passwordsMatch });

  private passwordsMatch(group: AbstractControl): { mismatch: boolean } | null {
    const newPassword = group.get('newPassword')?.value;
    const confirm = group.get('confirmPassword')?.value;
    return newPassword && confirm && newPassword !== confirm ? { mismatch: true } : null;
  }

  protected onSubmit(): void {
    if (this.form.invalid) return;

    this.saving = true;
    this.accountService.changePassword(
      this.form.value.currentPassword ?? '',
      this.form.value.newPassword ?? '',
    ).subscribe({
      next: () => {
        this.saving = false;
        this.toast.success('Contraseña actualizada. Vuelve a iniciar sesión.');
        this.authService.clearTokens();
        this.router.navigate(['/auth/login']);
      },
      error: () => this.saving = false,
    });
  }
}