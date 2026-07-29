import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import type { Category } from '../models/category';
import type { PagedResult } from '../models/paged-result';

@Injectable({ providedIn: 'root' })
export class CategoriesService {
  private http = inject(HttpClient);

  list(tree = false) {
    return this.http.get<Category[] | PagedResult<Category>>(`${environment.apiUrl}/categories`, {
      params: { ...(tree && { tree: 'true' }) },
    });
  }

  getBySlug(slug: string) {
    return this.http.get<Category>(`${environment.apiUrl}/categories/${slug}`);
  }

  create(data: Partial<Category>) {
    return this.http.post<Category>(`${environment.apiUrl}/admin/categories`, data);
  }

  update(id: string, data: Partial<Category>) {
    return this.http.put<Category>(`${environment.apiUrl}/admin/categories/${id}`, data);
  }

  delete(id: string) {
    return this.http.delete(`${environment.apiUrl}/admin/categories/${id}`);
  }
}
