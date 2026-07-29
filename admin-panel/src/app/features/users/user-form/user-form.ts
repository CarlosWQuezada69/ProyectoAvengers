import { Component, inject, OnInit } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { UsersService } from '../../../core/services/users.service';
import { ButtonComponent } from '../../../shared/components/button/button';
import { InputComponent } from '../../../shared/components/input/input';
import { ToastService } from '../../../shared/services/toast.service';
import type { Role } from '../../../core/models/role';

@Component({
  selector: 'app-user-form',
  imports: [ReactiveFormsModule, RouterLink, ButtonComponent, InputComponent],
  templateUrl: './user-form.html',
  styleUrl: './user-form.scss',
})
export class UserFormComponent implements OnInit {
  private usersService = inject(UsersService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private toast = inject(ToastService);

  protected editMode = false;
  protected userId: string | null = null;
  protected loading = false;
  protected roles: Role[] = [];
  protected selectedRoleIds: string[] = [];

  protected form = new FormGroup({
    firstName: new FormControl('', [Validators.required]),
    lastName: new FormControl('', [Validators.required]),
    email: new FormControl('', [Validators.required, Validators.email]),
    password: new FormControl(''),
    isActive: new FormControl(true),
  });

  ngOnInit(): void {
    this.userId = this.route.snapshot.paramMap.get('id');
    this.editMode = !!this.userId;

    this.usersService.getRoles().subscribe(data => this.roles = data);

    if (this.editMode && this.userId) {
      this.loading = true;
      this.usersService.getById(this.userId).subscribe({
        next: u => { this.form.patchValue(u); this.loading = false; },
        error: () => this.loading = false,
      });
    }
  }

  protected toggleRole(roleId: string): void {
    const idx = this.selectedRoleIds.indexOf(roleId);
    if (idx >= 0) this.selectedRoleIds.splice(idx, 1);
    else this.selectedRoleIds.push(roleId);
  }

  protected onSubmit(): void {
    if (this.form.invalid) return;

    if (this.editMode && this.userId) {
      this.usersService.update(this.userId, this.form.value as any).subscribe({
        next: () => {
          if (this.selectedRoleIds.length) {
            this.usersService.updateRoles(this.userId!, this.selectedRoleIds).subscribe();
          }
          this.toast.show('Usuario actualizado', 'success');
          this.router.navigate(['/users']);
        },
      });
    } else {
      this.usersService.create(this.form.value as any).subscribe({
        next: (res) => {
          if (this.selectedRoleIds.length) {
            this.usersService.updateRoles(res.id, this.selectedRoleIds).subscribe();
          }
          this.toast.show('Usuario creado', 'success');
          this.router.navigate(['/users']);
        },
      });
    }
  }
}
