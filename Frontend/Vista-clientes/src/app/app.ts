import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';

import { CatalogService } from './core/services/catalog.service';
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

  constructor() {
    // Precarga la configuración pública del sitio (branding, contacto, copyright)
    this.catalog.loadSettings().subscribe();
  }
}