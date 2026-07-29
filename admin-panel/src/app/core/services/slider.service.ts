import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import type { SliderItem } from '../models/index';

@Injectable({ providedIn: 'root' })
export class SliderService {
  private http = inject(HttpClient);

  list() {
    return this.http.get<SliderItem[]>(`${environment.apiUrl}/admin/slider`);
  }

  create(data: FormData) {
    return this.http.post<SliderItem>(`${environment.apiUrl}/admin/slider`, data);
  }

  update(id: string, data: Partial<SliderItem>) {
    return this.http.put<SliderItem>(`${environment.apiUrl}/admin/slider/${id}`, data);
  }

  delete(id: string) {
    return this.http.delete(`${environment.apiUrl}/admin/slider/${id}`);
  }

  reorder(order: { id: string; displayOrder: number }[]) {
    return this.http.put(`${environment.apiUrl}/admin/slider/order`, order);
  }
}
