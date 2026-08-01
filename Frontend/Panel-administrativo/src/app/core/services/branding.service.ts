import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { take } from 'rxjs/operators';

@Injectable({ providedIn: 'root' })
export class BrandingService {
  private http = inject(HttpClient);

  readonly logoUrl = signal('');
  readonly businessName = signal('Avengers');

  constructor() {
    this.refresh();
  }

  refresh(): void {
    this.http.get<Record<string, string>>(
      `${environment.apiUrl}/settings/public?t=${Date.now()}`
    ).pipe(take(1)).subscribe({
      next: settings => {
        if (settings['logo_url']) this.logoUrl.set(this.toAbsolute(settings['logo_url']));
        if (settings['business_name']) this.businessName.set(settings['business_name']);
      },
    });
  }

  private toAbsolute(url: string): string {
    if (!url || /^https?:\/\//.test(url)) return url;
    return `${environment.apiUrl.replace('/api/v1', '')}${url}`;
  }
}
