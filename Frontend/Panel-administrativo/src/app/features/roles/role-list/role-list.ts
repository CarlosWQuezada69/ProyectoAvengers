import { Component, inject, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { RolesService } from '../../../core/services/roles.service';
import { ButtonComponent } from '../../../shared/components/button/button';
import { BadgeComponent } from '../../../shared/components/badge/badge';
import { HasPermissionDirective } from '../../../shared/directives/has-permission.directive';
import { ToastService } from '../../../shared/services/toast.service';
import type { Role } from '../../../core/models/role';

@Component({
  selector: 'app-role-list',
  imports: [RouterLink, ButtonComponent, BadgeComponent, HasPermissionDirective],
  templateUrl: './role-list.html',
  styleUrl: './role-list.scss',
})
export class RoleListComponent implements OnInit {
  private rolesService = inject(RolesService);
  private toast = inject(ToastService);

  protected roles: Role[] = [];
  protected loading = true;

  ngOnInit(): void {
    this.rolesService.list().subscribe({
      next: data => { this.roles = data; this.loading = false; },
      error: () => this.loading = false,
    });
  }

  protected deleteRole(id: string): void {
    if (confirm('¿Eliminar este rol?')) {
      this.rolesService.delete(id).subscribe({
        next: () => {
          this.toast.show('Rol eliminado', 'success');
          this.roles = this.roles.filter(r => r.id !== id);
        },
        error: () => this.toast.show('No se puede eliminar: tiene usuarios asignados', 'error'),
      });
    }
  }
}
