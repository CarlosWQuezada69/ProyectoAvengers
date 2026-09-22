import { inject, Injectable, signal } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

import { environment } from '../../../environments/environment';
import { PagedResult } from '../models/paged-result';
import { ProductDto, ProductListDto, ProductQuery } from '../models/product';
import { CategoryDto } from '../models/category';
import { SliderItemDto } from '../models/slider';
import { PublicSettings } from '../models/settings';

@Injectable({ providedIn: 'root' })
export class CatalogService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = environment.apiUrl;

  private readonly settingsSignal = signal<PublicSettings | null>(null);
  readonly settings = this.settingsSignal.asReadonly();

  /** Precarga la configuración pública del sitio (con fallbacks en componentes). */
  loadSettings(): Observable<PublicSettings> {
    return this.getPublicSettings().pipe(tap((s) => this.settingsSignal.set(s)));
  }

  getFeaturedProducts(limit = 4): Observable<ProductListDto[]> {
    return this.http.get<ProductListDto[]>(`${this.apiUrl}/products/featured`, {
      params: new HttpParams().set('limit', String(limit))
    });
  }

  getProducts(query: ProductQuery = {}): Observable<PagedResult<ProductListDto>> {
    let params = new HttpParams();

    if (query.search) params = params.set('search', query.search);
    if (query.categoryId) params = params.set('categoryId', query.categoryId);
    if (query.minPrice != null) params = params.set('minPrice', String(query.minPrice));
    if (query.maxPrice != null) params = params.set('maxPrice', String(query.maxPrice));
    if (query.onlyAvailable) params = params.set('onlyAvailable', String(query.onlyAvailable));
    if (query.sort) params = params.set('sort', query.sort);
    params = params.set('page', String(query.page ?? 1));
    params = params.set('pageSize', String(query.pageSize ?? 12));

    return this.http.get<PagedResult<ProductListDto>>(`${this.apiUrl}/products`, { params });
  }

  getProduct(slug: string): Observable<ProductDto> {
    return this.http.get<ProductDto>(`${this.apiUrl}/products/${slug}`);
  }

  trackView(productId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/products/${productId}/track-view`, null);
  }

  getSlider(): Observable<SliderItemDto[]> {
    return this.http.get<SliderItemDto[]>(`${this.apiUrl}/slider`);
  }

  getCategories(): Observable<CategoryDto[]> {
    return this.http.get<CategoryDto[]>(`${this.apiUrl}/categories`, {
      params: new HttpParams().set('tree', 'true')
    });
  }

  getPublicSettings(): Observable<PublicSettings> {
    return this.http.get<PublicSettings>(`${this.apiUrl}/settings/public`);
  }
}