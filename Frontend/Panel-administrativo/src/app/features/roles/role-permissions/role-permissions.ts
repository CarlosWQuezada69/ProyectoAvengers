import { Component, inject, OnInit, ChangeDetectionStrategy, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { RolesService, PermissionsService } from '../../../core/services/roles.service';
import { ButtonComponent } from '../../../shared/components/button/button';
import { BadgeComponent } from '../../../shared/components/badge/badge';
import { ToastService } from '../../../shared/services/toast.service';
import type { Role } from '../../../core/models/role';
import type { Permission } from '../../../core/models/permission';

@Component({
  selector: 'app-role-permissions',
  imports: [FormsModule, ButtonComponent],
  templateUrl: './role-permissions.html',
  styleUrl: './role-permissions.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RolePermissionsComponent implements OnInit {
  private rolesService = inject(RolesService);
  private permissionsService = inject(PermissionsService);
  private route = inject(ActivatedRoute);
  private toast = inject(ToastService);

  protected role = signal<Role | null>(null);
  protected permissions = signal<Permission[]>([]);
  protected selectedIds: string[] = [];
  protected loading = true;

  private roleId = this.route.snapshot.paramMap.get('id') ?? '';

  ngOnInit(): void {
    this.rolesService.list().subscribe(roles => {
      this.role.set(roles.find(r => r.id === this.roleId) ?? null);
    });
    this.permissionsService.list().subscribe({
      next: data => { this.permissions.set(data); this.loading = false; },
      error: () => this.loading = false,
    });
  }

  protected groupedPermissions(): { module: string; perms: Permission[] }[] {
    const groups = new Map<string, Permission[]>();
    for (const p of this.permissions()) {
      if (!groups.has(p.module)) groups.set(p.module, []);
      groups.get(p.module)!.push(p);
    }
    return Array.from(groups.entries()).map(([module, perms]) => ({ module, perms }));
  }

  protected toggle(permId: string): void {
    const idx = this.selectedIds.indexOf(permId);
    if (idx >= 0) this.selectedIds.splice(idx, 1);
    else this.selectedIds.push(permId);
  }

  protected save(): void {
    this.rolesService.updatePermissions(this.roleId, this.selectedIds).subscribe(() => {
      this.toast.show('Permisos actualizados', 'success');
    });
  }
}
