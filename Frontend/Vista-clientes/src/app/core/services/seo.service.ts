import { DOCUMENT } from '@angular/common';
import { inject, Injectable } from '@angular/core';

import { PublicSettings } from '../models/settings';

const DEFAULT_TITLE = 'The Avengers Joyero | Ediciones limitadas';
const DEFAULT_DESCRIPTION =
  'Joyería de edición limitada inspirada en los héroes más poderosos de la Tierra. Colecciones numeradas, acabados premium y envíos cuidados.';

@Injectable({ providedIn: 'root' })
export class SeoService {
  private readonly document = inject(DOCUMENT);

  /** Aplica el branding y SEO configurados desde el panel administrativo. */
  apply(settings: PublicSettings | null): void {
    const title = settings?.seo_title?.trim() || DEFAULT_TITLE;
    const description = settings?.seo_description?.trim() || DEFAULT_DESCRIPTION;
    const keywords = settings?.seo_keywords?.trim() || 'joyería, edición limitada, avengers, colecciones';
    const business = settings?.business_name?.trim();

    this.setMeta('title', title);
    this.document.title = title;

    const finalDescription = description + (business ? ` — ${business}` : '');
    this.setMeta('description', finalDescription);
    this.setMeta('keywords', keywords);
    this.setMeta('og:title', title);
    this.setProperty('og:description', finalDescription);
    this.setMeta('twitter:title', title);
    this.setMeta('twitter:description', finalDescription);
  }

  private queryMeta(name: string): HTMLMetaElement | null {
    return this.document.querySelector<HTMLMetaElement>(`meta[name="${name}"], meta[property="${name}"]`);
  }

  private setMeta(name: string, content: string): void {
    const existing = this.queryMeta(name);
    if (existing) {
      existing.setAttribute('content', content);
      return;
    }
    const meta = this.document.createElement('meta');
    if (name.startsWith('og:') || name.startsWith('twitter:')) {
      meta.setAttribute('property', name);
    } else {
      meta.setAttribute('name', name);
    }
    meta.setAttribute('content', content);
    this.document.head.appendChild(meta);
  }

  private setProperty(property: string, content: string): void {
    const existing = this.document.querySelector<HTMLMetaElement>(
      `meta[property="${property}"]`
    );
    if (existing) {
      existing.setAttribute('content', content);
      return;
    }
    const meta = this.document.createElement('meta');
    meta.setAttribute('property', property);
    meta.setAttribute('content', content);
    this.document.head.appendChild(meta);
  }
}