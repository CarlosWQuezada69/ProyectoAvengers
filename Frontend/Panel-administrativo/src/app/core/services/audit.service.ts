import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import type { AuditLog } from '../models/index';
import type { PagedResult } from '../models/paged-result';

export interface AuditLogFilters {
  userId?: string;
  entityName?: string;
  from?: string;
  to?: string;
  page?: number;
  pageSize?: number;
}

@Injectable({ providedIn: 'root' })
export class AuditService {
  private http = inject(HttpClient);

  list(filters: AuditLogFilters = {}) {
    let params = new HttpParams();
    for (const [key, value] of Object.entries(filters)) {
      if (value !== undefined && value !== null && value !== '') {
        params = params.set(key, String(value));
      }
    }
    return this.http.get<PagedResult<AuditLog>>(`${environment.apiUrl}/admin/audit-logs`, { params });
  }
}