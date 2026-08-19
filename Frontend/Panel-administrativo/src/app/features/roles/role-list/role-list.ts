import { Component, inject, OnInit, ChangeDetectionStrategy, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { RolesService } from '../../../core/services/roles.service';
import { AuthService } from '../../../core/services/auth.service';
import { ButtonComponent } from '../../../shared/components/button/button';
import { BadgeComponent } from '../../../shared/components/badge/badge';
import { HasPermissionDirective } from '../../../shared/directives/has-permission.directive';
import { ConfirmDialogService } from '../../../shared/services/confirm-dialog.service';
import { ToastService } from '../../../shared/services/toast.service';
import type { Role } from '../../../core/models/role';

@Component({
  selector: 'app-role-list',
  imports: [RouterLink, ButtonComponent, BadgeComponent, HasPermissionDirective],
  templateUrl: './role-list.html',
  styleUrl: './role-list.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RoleListComponent implements OnInit {
  private rolesService = inject(RolesService);
  private authService = inject(AuthService);
  private toast = inject(ToastService);
  private confirmDialog = inject(ConfirmDialogService);

  protected roles = signal<Role[]>([]);
  protected loading = signal(true);

  protected readonly currentUser = this.authService.user;
  protected readonly myPermissions = this.authService.permissions;

  ngOnInit(): void {
    this.rolesService.list().subscribe({
      next: data => { this.roles.set(data); this.loading.set(false);
        if (data.length === 0) this.toast.info('Aún no hay roles registrados');
      },
      error: () => this.loading.set(false),
    });
  }

  protected async deleteRole(id: string): Promise<void> {
    const confirmed = await this.confirmDialog.confirm('¿Eliminar este rol?');
    if (!confirmed) return;
    this.rolesService.delete(id).subscribe({
      next: () => {
        this.toast.show('Rol eliminado', 'success');
        this.roles.update(r => r.filter(role => role.id !== id));
      },
      error: () => this.toast.show('No se puede eliminar: tiene usuarios asignados', 'error'),
    });
  }
}
