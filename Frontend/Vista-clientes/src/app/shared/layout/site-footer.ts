import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { environment } from '../../../environments/environment';
import { CatalogService } from '../../core/services/catalog.service';
import { CategoryDto } from '../../core/models/category';
import { AssetUrlPipe } from '../../core/pipes/asset-url.pipe';
import { parseSocialLinks, SocialLink } from '../../core/utils/social';
import { IconComponent } from '../icon/icon';
import { RevealDirective } from '../../shared/directives/reveal.directive';

@Component({
  selector: 'app-site-footer',
  standalone: true,
  imports: [IconComponent, RouterLink, AssetUrlPipe, RevealDirective],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './site-footer.html',
  styleUrls: ['./site-footer.scss']
})
export class SiteFooterComponent {
  private readonly catalog = inject(CatalogService);

  readonly adminUrl = environment.adminUrl;
  readonly categories = signal<CategoryDto[]>([]);

  readonly logoUrl = computed(() => this.catalog.settings()?.logo_url?.trim() || null);
  readonly businessName = computed(() => this.catalog.settings()?.business_name ?? 'The Avengers Joyero');
  readonly contactEmail = computed(() => this.catalog.settings()?.contact_email ?? 'hola@avengersjoyero.com');
  readonly contactPhone = computed(() => this.catalog.settings()?.contact_phone ?? '+1 809 000 0000');
  readonly copyright = computed(
    () => this.catalog.settings()?.copyright_text ?? '© 2024 The Avengers Joyero'
  );
  readonly socials = computed<SocialLink[]>(() =>
    parseSocialLinks(this.catalog.settings()?.social_links ?? null)
  );

  constructor() {
    this.catalog
      .getCategories()
      .pipe(takeUntilDestroyed())
      .subscribe((cats) => this.categories.set(cats.slice(0, 6)));
  }
}