import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

import { CatalogService } from '../../core/services/catalog.service';
import { AboutInfoDto } from '../../core/models/about';
import { AssetUrlPipe } from '../../core/pipes/asset-url.pipe';
import {
  buildWhatsAppLink,
  resolveWhatsAppNumber
} from '../../core/utils/whatsapp';
import { IconComponent } from '../../shared/icon/icon';

const FALLBACK_TITLE = 'Acerca de The Avengers Joyero';
const FALLBACK_HISTORY = `The Avengers Joyero nació de la unión entre la pasión por el cómic y la joyería de autor.
Cada pieza es una edición limitada inspirada en los héroes más poderosos de la Tierra, elaborada a mano con materiales nobles y acabados que buscan tributar a la leyenda.
Creemos que el poder también se lleva puesto: por eso diseñamos colecciones que combinan la estética oscura y premium con destellos de oro, tal y como merece un héroe.`;
import { RevealDirective } from '../../shared/directives/reveal.directive';

@Component({
  selector: 'app-about-page',
  standalone: true,
  imports: [IconComponent, RouterLink, AssetUrlPipe, RevealDirective],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './about-page.html',
  styleUrls: ['./about-page.scss']
})
export class AboutPageComponent {
  private readonly catalog = inject(CatalogService);

  readonly about = signal<AboutInfoDto | null>(null);
  readonly loading = signal(true);

  readonly title = computed(() => this.about()?.title?.trim() || FALLBACK_TITLE);
  readonly story = computed(() => this.about()?.history?.trim() || FALLBACK_HISTORY);
  readonly mission = computed(() => this.about()?.mission?.trim() || null);
  readonly vision = computed(() => this.about()?.vision?.trim() || null);
  readonly paragraphs = computed(() =>
    this.story()
      .split(/\n\s*\n|\r\n\r\n/)
      .map((p) => p.trim())
      .filter(Boolean)
  );

  readonly contactEmail = computed(() => this.catalog.settings()?.contact_email ?? 'hola@avengersjoyero.com');
  readonly contactPhone = computed(() => this.catalog.settings()?.contact_phone ?? '+1 809 000 0000');
  readonly whatsappHref = computed(() => {
    const phone = resolveWhatsAppNumber(this.catalog.settings());
    return phone ? buildWhatsAppLink(phone, 'Hola, me gustaría recibir más información.') : null;
  });

  constructor() {
    this.catalog.getAbout().subscribe({
      next: (about) => {
        if (about && (about.title?.trim() || about.history?.trim())) {
          this.about.set(about);
        }
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }
}