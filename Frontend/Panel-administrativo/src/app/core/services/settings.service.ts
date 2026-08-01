import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import type { SiteSetting } from '../models/index';

@Injectable({ providedIn: 'root' })
export class SettingsService {
  private http = inject(HttpClient);

  getPublic() {
    return this.http.get<SiteSetting[]>(`${environment.apiUrl}/settings/public`);
  }

  getAll() {
    return this.http.get<SiteSetting[]>(`${environment.apiUrl}/admin/settings`);
  }

  update(key: string, value: string) {
    return this.http.put(`${environment.apiUrl}/admin/settings/${key}`, { value });
  }

  uploadLogo(file: File) {
    const fd = new FormData();
    fd.append('file', file);
    return this.http.post<SiteSetting>(`${environment.apiUrl}/admin/settings/logo`, fd);
  }
}
