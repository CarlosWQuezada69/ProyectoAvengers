import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import type { AuditLog } from '../models/index';
import type { PagedResult } from '../models/paged-result';

@Injectable({ providedIn: 'root' })
export class AuditService {
  private http = inject(HttpClient);

  list(params?: { userId?: string; entityName?: string; from?: string; to?: string; page?: number; pageSize?: number }) {
    return this.http.get<PagedResult<AuditLog>>(`${environment.apiUrl}/admin/audit-logs`, { params: params as any });
  }
}
