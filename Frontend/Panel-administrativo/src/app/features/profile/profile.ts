import { Component, inject, ChangeDetectionStrategy } from '@angular/core';
import { AbstractControl, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AccountService } from '../../core/services/account.service';
import { AuthService } from '../../core/services/auth.service';
import { Router } from '@angular/router';
import { ButtonComponent } from '../../shared/components/button/button';
import { InputComponent } from '../../shared/components/input/input';
import { BadgeComponent } from '../../shared/components/badge/badge';
import { ToastService } from '../../shared/services/toast.service';

@Component({
  selector: 'app-profile',
  imports: [ReactiveFormsModule, ButtonComponent, InputComponent, BadgeComponent],
  templateUrl: './profile.html',
  styleUrl: './profile.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProfileComponent {
  private accountService = inject(AccountService);
  private authService = inject(AuthService);
  private router = inject(Router);
  private toast = inject(ToastService);

  protected email = '';
  protected emailConfirmed = false;
  protected roles: string[] = [];
  protected saving = false;
  protected sendingEmailChange = false;
  protected confirmationUrl = '';

  protected form = new FormGroup({
    firstName: new FormControl('', [Validators.required, Validators.maxLength(100)]),
    lastName: new FormControl('', [Validators.required, Validators.maxLength(100)]),
    phone: new FormControl('', [Validators.maxLength(20)]),
  });

  protected emailForm = new FormGroup({
    newEmail: new FormControl('', [Validators.required, Validators.email]),
  });

  protected passwordForm = new FormGroup({
    currentPassword: new FormControl('', [Validators.required]),
    newPassword: new FormControl('', [Validators.required, Validators.minLength(8)]),
    confirmPassword: new FormControl('', [Validators.required]),
  }, { validators: this.passwordsMatch });

  constructor() {
    this.accountService.getProfile().subscribe({
      next: profile => {
        this.email = profile.email;
        this.emailConfirmed = profile.emailConfirmed;
        this.roles = profile.roles;
        this.form.patchValue({
          firstName: profile.firstName,
          lastName: profile.lastName,
          phone: profile.phone ?? '',
        });
      },
    });
  }

  private passwordsMatch(group: AbstractControl): { mismatch: boolean } | null {
    const newPassword = group.get('newPassword')?.value;
    const confirm = group.get('confirmPassword')?.value;
    return newPassword && confirm && newPassword !== confirm ? { mismatch: true } : null;
  }

  protected onSubmit(): void {
    if (this.form.invalid) return;

    this.saving = true;
    this.accountService.updateProfile({
      firstName: this.form.value.firstName ?? '',
      lastName: this.form.value.lastName ?? '',
      phone: this.form.value.phone || undefined,
    }).subscribe({
      next: () => {
        this.saving = false;
        this.toast.success('Perfil actualizado correctamente');
        this.authService.loadUser();
      },
      error: () => this.saving = false,
    });
  }

  protected onChangeEmail(): void {
    if (this.emailForm.invalid) return;

    this.sendingEmailChange = true;
    this.confirmationUrl = '';
    this.accountService.changeEmail(this.emailForm.value.newEmail ?? '').subscribe({
      next: res => {
        this.sendingEmailChange = false;
        this.toast.success(res.message);
        if (res.confirmationUrl) {
          this.confirmationUrl = res.confirmationUrl;
        }
        this.emailForm.reset();
      },
      error: () => this.sendingEmailChange = false,
    });
  }

  protected onChangePassword(): void {
    if (this.passwordForm.invalid) return;

    this.accountService.changePassword(
      this.passwordForm.value.currentPassword ?? '',
      this.passwordForm.value.newPassword ?? '',
    ).subscribe({
      next: () => {
        this.toast.success('Contraseña actualizada. Vuelve a iniciar sesión.');
        this.authService.clearTokens();
        this.router.navigate(['/auth/login']);
      },
    });
  }
}