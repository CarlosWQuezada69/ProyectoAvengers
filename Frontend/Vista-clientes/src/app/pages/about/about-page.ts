import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';

import { CatalogService } from '../../core/services/catalog.service';
import { IconComponent } from '../../shared/icon/icon';

@Component({
  selector: 'app-about-page',
  standalone: true,
  imports: [IconComponent, RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './about-page.html',
  styleUrls: ['./about-page.scss']
})
export class AboutPageComponent {
  private readonly catalog = inject(CatalogService);

  readonly contactEmail = computed(() => this.catalog.settings()?.contact_email ?? 'hola@avengersjoyero.com');
  readonly contactPhone = computed(() => this.catalog.settings()?.contact_phone ?? '+34 600 000 000');
}