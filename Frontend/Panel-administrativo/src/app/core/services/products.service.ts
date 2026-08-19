import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import type { Product, ProductImage, ProductRestriction } from '../models/product';
import type { PagedResult } from '../models/paged-result';

@Injectable({ providedIn: 'root' })
export class ProductsService {
  private http = inject(HttpClient);

  list(params?: {
    search?: string;
    categoryId?: string;
    isActive?: boolean;
    page?: number;
    pageSize?: number;
    sortBy?: string;
    sortDir?: string;
  }) {
    let httpParams = new HttpParams()
      .set('page', params?.page ?? 1)
      .set('pageSize', params?.pageSize ?? 20);
    if (params?.search) httpParams = httpParams.set('search', params.search);
    if (params?.categoryId) httpParams = httpParams.set('categoryId', params.categoryId);
    if (params?.isActive !== undefined) httpParams = httpParams.set('isActive', String(params.isActive));
    if (params?.sortBy) httpParams = httpParams.set('sortBy', params.sortBy);
    if (params?.sortDir) httpParams = httpParams.set('sortDir', params.sortDir);

    return this.http.get<PagedResult<Product>>(`${environment.apiUrl}/admin/products`, { params: httpParams });
  }

  getById(id: string) {
    return this.http.get<Product>(`${environment.apiUrl}/admin/products/${id}`);
  }

  create(data: Partial<Product>) {
    return this.http.post<Product>(`${environment.apiUrl}/admin/products`, data);
  }

  update(id: string, data: Partial<Product>) {
    return this.http.put<Product>(`${environment.apiUrl}/admin/products/${id}`, data);
  }

  delete(id: string) {
    return this.http.delete(`${environment.apiUrl}/admin/products/${id}`);
  }

  uploadImage(productId: string, file: File) {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<ProductImage>(`${environment.apiUrl}/admin/products/${productId}/images`, formData);
  }

  deleteImage(productId: string, imageId: string) {
    return this.http.delete(`${environment.apiUrl}/admin/products/${productId}/images/${imageId}`);
  }

  reorderImages(productId: string, order: { imageId: string; displayOrder: number }[]) {
    return this.http.put(`${environment.apiUrl}/admin/products/${productId}/images/order`, order);
  }

  addRestriction(productId: string, data: Partial<ProductRestriction>) {
    return this.http.post<ProductRestriction>(`${environment.apiUrl}/admin/products/${productId}/restrictions`, data);
  }

  updateRestriction(productId: string, restrictionId: string, data: Partial<ProductRestriction>) {
    return this.http.put<ProductRestriction>(`${environment.apiUrl}/admin/products/${productId}/restrictions/${restrictionId}`, data);
  }

  deleteRestriction(productId: string, restrictionId: string) {
    return this.http.delete(`${environment.apiUrl}/admin/products/${productId}/restrictions/${restrictionId}`);
  }
}
