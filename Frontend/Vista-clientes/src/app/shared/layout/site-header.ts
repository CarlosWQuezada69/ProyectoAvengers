import { ChangeDetectionStrategy, Component, HostListener, computed, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { IconComponent } from '../icon/icon';
import { AssetUrlPipe } from '../../core/pipes/asset-url.pipe';
import { CartService } from '../../core/services/cart.service';
import { CatalogService } from '../../core/services/catalog.service';
import { CategoryDto } from '../../core/models/category';

interface StaticLink {
  label: string;
  href: string;
  query?: Record<string, string>;
  fragment?: string;
}

const STATIC_LINKS: StaticLink[] = [
  { label: 'Inicio', href: '/' },
  { label: 'Productos', href: '/productos' },
  { label: 'Acerca de', href: '/acerca' },
  { label: 'Contacto', href: '/', fragment: 'contacto' }
];

@Component({
  selector: 'app-site-header',
  standalone: true,
  imports: [IconComponent, RouterLink, AssetUrlPipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './site-header.html',
  styleUrls: ['./site-header.scss']
})
export class SiteHeaderComponent {
  private readonly router = inject(Router);
  private readonly cart = inject(CartService);
  private readonly catalog = inject(CatalogService);

  readonly navLinks = STATIC_LINKS;
  readonly categories = signal<CategoryDto[]>([]);

  readonly logoUrl = computed(() => this.catalog.settings()?.logo_url?.trim() || null);
  readonly businessName = computed(
    () => this.catalog.settings()?.business_name?.trim() || 'The Avengers Joyero'
  );

  readonly menuOpen = signal(false);
  readonly collectionsOpen = signal(false);
  readonly mobileCollectionsOpen = signal(false);
  readonly scrolled = signal(false);

  readonly cartCount = this.cart.count;

  constructor() {
    this.catalog
      .getCategories()
      .pipe(takeUntilDestroyed())
      .subscribe((cats) => this.categories.set(cats));

    this.router.events.pipe(takeUntilDestroyed()).subscribe(() => {
      this.menuOpen.set(false);
      this.collectionsOpen.set(false);
    });
  }

  @HostListener('window:scroll')
  protected onScroll(): void {
    this.scrolled.set((window.scrollY ?? 0) > 8);
  }

  /** Cierra el dropdown de colecciones si se hace clic fuera de él. */
  @HostListener('document:click', ['$event'])
  protected onClickOutside(event: MouseEvent): void {
    const el = event.target as HTMLElement;
    if (this.collectionsOpen() && !el.closest('.site-header__collections')) {
      this.collectionsOpen.set(false);
    }
  }

  protected goToCategory(category: CategoryDto): void {
    this.collectionsOpen.set(false);
    this.mobileCollectionsOpen.set(false);
    this.menuOpen.set(false);
    this.router.navigate(['/productos'], { queryParams: { cat: category.id } });
  }

  protected toggleMenu(): void {
    this.menuOpen.update((open) => !open);
  }

  protected toggleCollections(): void {
    this.collectionsOpen.update((open) => !open);
  }

  protected toggleMobileCollections(): void {
    this.mobileCollectionsOpen.update((open) => !open);
  }

  protected openCart(): void {
    this.cart.open();
  }
}