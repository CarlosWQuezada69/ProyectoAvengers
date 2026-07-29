import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
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
    return this.http.get<PagedResult<Product>>(`${environment.apiUrl}/admin/products`, { params: params as any });
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
