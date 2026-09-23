import { Component, inject } from '@angular/core';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { filter } from 'rxjs';

import { CatalogService } from './core/services/catalog.service';
import { SeoService } from './core/services/seo.service';
import { SiteHeaderComponent } from './shared/layout/site-header';
import { SiteFooterComponent } from './shared/layout/site-footer';
import { CartDrawerComponent } from './shared/layout/cart-drawer';
import { WhatsAppFloatComponent } from './shared/components/whatsapp-float/whatsapp-float';

@Component({
  selector: 'app-root',
  imports: [
    RouterOutlet,
    SiteHeaderComponent,
    SiteFooterComponent,
    CartDrawerComponent,
    WhatsAppFloatComponent
  ],
  templateUrl: './app.html',
  styleUrls: ['./app.scss']
})
export class App {
  private readonly catalog = inject(CatalogService);
  private readonly seo = inject(SeoService);
  private readonly router = inject(Router);

  constructor() {
    // Precarga la configuración pública del sitio (branding, contacto, copyright) y aplica SEO
    this.refreshSettings();

    // Re-sincroniza en cada navegación para reflejar cambios hechos en el panel admin
    this.router.events
      .pipe(
        filter((event): event is NavigationEnd => event instanceof NavigationEnd),
        takeUntilDestroyed()
      )
      .subscribe((event) => {
        this.refreshSettings();
        this.sendPageView(event.url);
      });
  }

  private refreshSettings(): void {
    this.catalog.loadSettings().subscribe((settings) => this.seo.apply(settings));
  }

  private sendPageView(url: string): void {
    const path = url.split('?')[0];
    const segment = path.split('/').filter(Boolean)[0];

    let page: string;
    if (!segment) page = 'home';
    else if (segment === 'productos') page = 'catalog';
    else if (segment === 'producto') page = 'product';
    else if (segment === 'acerca-de') page = 'about';
    else page = segment;

    this.catalog.trackPageView(page).subscribe();
  }
}