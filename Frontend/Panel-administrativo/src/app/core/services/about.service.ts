import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import type { AboutInfo, AboutGallery, UpdateAboutInfoRequest } from '../models/about';

@Injectable({ providedIn: 'root' })
export class AboutService {
  private http = inject(HttpClient);

  getAbout() {
    return this.http.get<AboutInfo>(`${environment.apiUrl}/admin/about`);
  }

  updateAbout(data: UpdateAboutInfoRequest) {
    return this.http.put<AboutInfo>(`${environment.apiUrl}/admin/about`, data);
  }

  uploadGallery(file: File, section: string) {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<AboutGallery>(`${environment.apiUrl}/admin/about/gallery`, formData, {
      params: new HttpParams().set('section', section),
    });
  }

  deleteGallery(id: string) {
    return this.http.delete(`${environment.apiUrl}/admin/about/gallery/${id}`);
  }

  reorderGallery(order: { id: string; displayOrder: number }[]) {
    return this.http.put(`${environment.apiUrl}/admin/about/gallery/order`, order);
  }
}