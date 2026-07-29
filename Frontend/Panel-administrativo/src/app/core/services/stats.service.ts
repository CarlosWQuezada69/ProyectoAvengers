import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import type { StatsOverview, TopProduct } from '../models/index';

@Injectable({ providedIn: 'root' })
export class StatsService {
  private http = inject(HttpClient);

  getOverview() {
    return this.http.get<StatsOverview>(`${environment.apiUrl}/admin/stats/overview`);
  }

  getTopViewed(from?: string, to?: string, limit = 10) {
    return this.http.get<TopProduct[]>(`${environment.apiUrl}/admin/stats/products/top-viewed`, {
      params: { ...(from && { from }), ...(to && { to }), limit },
    });
  }

  getTopSellers(from?: string, to?: string, limit = 10) {
    return this.http.get<TopProduct[]>(`${environment.apiUrl}/admin/stats/products/top-sellers`, {
      params: { ...(from && { from }), ...(to && { to }), limit },
    });
  }

  getLowStock(threshold = 10) {
    return this.http.get<TopProduct[]>(`${environment.apiUrl}/admin/stats/products/low-stock`, {
      params: { threshold },
    });
  }
}
