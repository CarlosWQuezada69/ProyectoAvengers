import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import type { Role } from '../models/role';
import type { Permission } from '../models/permission';

@Injectable({ providedIn: 'root' })
export class RolesService {
  private http = inject(HttpClient);

  list() {
    return this.http.get<Role[]>(`${environment.apiUrl}/admin/roles`);
  }

  create(data: Partial<Role>) {
    return this.http.post<Role>(`${environment.apiUrl}/admin/roles`, data);
  }

  update(id: string, data: Partial<Role>) {
    return this.http.put<Role>(`${environment.apiUrl}/admin/roles/${id}`, data);
  }

  delete(id: string) {
    return this.http.delete(`${environment.apiUrl}/admin/roles/${id}`);
  }

  updatePermissions(id: string, permissionIds: string[]) {
    return this.http.put(`${environment.apiUrl}/admin/roles/${id}/permissions`, { permissionIds });
  }
}

@Injectable({ providedIn: 'root' })
export class PermissionsService {
  private http = inject(HttpClient);

  list() {
    return this.http.get<Permission[]>(`${environment.apiUrl}/admin/permissions`);
  }
}
