import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import type { User } from '../models/user';
import type { PagedResult } from '../models/paged-result';
import type { Role } from '../models/role';

@Injectable({ providedIn: 'root' })
export class UsersService {
  private http = inject(HttpClient);

  list(params?: { search?: string; roleId?: string; isActive?: boolean; page?: number; pageSize?: number }) {
    let httpParams = new HttpParams()
      .set('page', params?.page ?? 1)
      .set('pageSize', params?.pageSize ?? 20);
    if (params?.search) httpParams = httpParams.set('search', params.search);
    if (params?.roleId) httpParams = httpParams.set('roleId', params.roleId);
    if (params?.isActive !== undefined) httpParams = httpParams.set('isActive', String(params.isActive));

    return this.http.get<PagedResult<User>>(`${environment.apiUrl}/admin/users`, { params: httpParams });
  }

  getById(id: string) {
    return this.http.get<User>(`${environment.apiUrl}/admin/users/${id}`);
  }

  create(data: Partial<User>) {
    return this.http.post<User>(`${environment.apiUrl}/admin/users`, data);
  }

  update(id: string, data: Partial<User>) {
    return this.http.put<User>(`${environment.apiUrl}/admin/users/${id}`, data);
  }

  delete(id: string) {
    return this.http.delete(`${environment.apiUrl}/admin/users/${id}`);
  }

  updateRoles(id: string, roleIds: string[]) {
    return this.http.put(`${environment.apiUrl}/admin/users/${id}/roles`, { roleIds });
  }

  getRoles() {
    return this.http.get<Role[]>(`${environment.apiUrl}/admin/roles`);
  }
}
