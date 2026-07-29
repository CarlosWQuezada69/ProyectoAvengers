import { Injectable, inject, signal, DestroyRef } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { take } from 'rxjs/operators';

@Injectable({ providedIn: 'root' })
export class BrandingService {
  private http = inject(HttpClient);

  readonly logoUrl = signal('');
  readonly businessName = signal('Avengers');

  constructor() {
    this.http.get<{ key: string; value?: string }[]>(
      `${environment.apiUrl}/settings/public`
    ).pipe(take(1)).subscribe({
      next: settings => {
        const logo = settings.find(x => x.key === 'logo_url')?.value;
        if (logo) this.logoUrl.set(logo);
        const name = settings.find(x => x.key === 'business_name')?.value;
        if (name) this.businessName.set(name);
      },
    });
  }
}
