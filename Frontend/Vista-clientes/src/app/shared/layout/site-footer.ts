import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';

import { CatalogService } from '../../core/services/catalog.service';
import { IconComponent } from '../icon/icon';

@Component({
  selector: 'app-site-footer',
  standalone: true,
  imports: [IconComponent, RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './site-footer.html',
  styleUrls: ['./site-footer.scss']
})
export class SiteFooterComponent {
  private readonly catalog = inject(CatalogService);

  readonly contactEmail = computed(() => this.catalog.settings()?.contact_email ?? 'hola@avengersjoyero.com');
  readonly contactPhone = computed(() => this.catalog.settings()?.contact_phone ?? '+34 600 000 000');
  readonly copyright = computed(
    () => this.catalog.settings()?.copyright_text ?? '© 2024 MARVEL JOYERO S.L.'
  );
}