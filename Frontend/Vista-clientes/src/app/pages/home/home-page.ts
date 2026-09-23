import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DecimalPipe } from '@angular/common';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { interval } from 'rxjs';

import { CatalogService } from '../../core/services/catalog.service';
import { CartService } from '../../core/services/cart.service';
import { ProductListDto } from '../../core/models/product';
import { SliderItemDto } from '../../core/models/slider';
import { IconComponent } from '../../shared/icon/icon';
import { ProductCardComponent } from '../../shared/components/product-card/product-card';
import { ProductImageFallbackComponent } from '../../shared/components/product-image-fallback/product-image-fallback';
import { RevealDirective } from '../../shared/directives/reveal.directive';

interface HeroSlide {
  id: string;
  kicker: string;
  titleLine1: string;
  titleLine2: string;
  imageUrl: string | null;
  productRef: { name: string };
  theme: 'shield' | 'reactor' | 'mjolnir' | 'gems';
  linkUrl: string | null;
}

const FALLBACK_SLIDES: HeroSlide[] = [
  {
    id: 'fallback-1',
    kicker: 'Edición limitada:',
    titleLine1: 'AVENGERS',
    titleLine2: 'ASSEMBLE!',
    imageUrl: null,
    productRef: { name: 'Colgante Capitán América' },
    theme: 'shield',
    linkUrl: '/productos'
  },
  {
    id: 'fallback-2',
    kicker: 'Tecnología y lujo:',
    titleLine1: 'ARC REACTOR',
    titleLine2: 'DE HIERRO',
    imageUrl: null,
    productRef: { name: 'Anillo Arc Reactor' },
    theme: 'reactor',
    linkUrl: '/productos'
  },
  {
    id: 'fallback-3',
    kicker: 'El poder de asgard:',
    titleLine1: 'FUERZA',
    titleLine2: 'DE THOR',
    imageUrl: null,
    productRef: { name: 'Brazalete Martillo Thor' },
    theme: 'mjolnir',
    linkUrl: '/productos'
  },
  {
    id: 'fallback-4',
    kicker: 'Seis gemas, un destino:',
    titleLine1: 'PODER',
    titleLine2: 'INFINITO',
    imageUrl: null,
    productRef: { name: 'Colgante Gemas del Infinito' },
    theme: 'gems',
    linkUrl: '/productos'
  }
];

const FALLBACK_PRODUCTS: ProductListDto[] = [
  {
    id: '00000000-0000-4000-8000-000000000001',
    sku: 'FALLBACK-CA-001',
    name: 'Colgante Capitán América',
    slug: 'colgante-capitan-america',
    price: 149.99,
    compareAtPrice: null,
    stock: 12,
    categoryName: 'Colecciones',
    isActive: true,
    isFeatured: true,
    primaryImageUrl: null,
    createdAt: new Date().toISOString()
  },
  {
    id: '00000000-0000-4000-8000-000000000002',
    sku: 'FALLBACK-AR-002',
    name: 'Anillo Arc Reactor Iron Man',
    slug: 'anillo-arc-reactor-iron-man',
    price: 149.99,
    compareAtPrice: 179.99,
    stock: 8,
    categoryName: 'Colecciones',
    isActive: true,
    isFeatured: true,
    primaryImageUrl: null,
    createdAt: new Date().toISOString()
  },
  {
    id: '00000000-0000-4000-8000-000000000003',
    sku: 'FALLBACK-MT-003',
    name: 'Brazalete Martillo Thor',
    slug: 'brazalete-martillo-thor',
    price: 149.99,
    compareAtPrice: null,
    stock: 20,
    categoryName: 'Colecciones',
    isActive: true,
    isFeatured: true,
    primaryImageUrl: null,
    createdAt: new Date().toISOString()
  },
  {
    id: '00000000-0000-4000-8000-000000000004',
    sku: 'FALLBACK-GI-004',
    name: 'Colgante Gemas del Infinito',
    slug: 'colgante-gemas-del-infinito',
    price: 149.99,
    compareAtPrice: 189.99,
    stock: 5,
    categoryName: 'Colecciones',
    isActive: true,
    isFeatured: true,
    primaryImageUrl: null,
    createdAt: new Date().toISOString()
  }
];

function toSlide(item: SliderItemDto, index: number): HeroSlide {
  const words = item.title.trim().split(/\s+/);
  const line1 = words.slice(0, Math.ceil(words.length / 2)).join(' ');
  const line2 = words.slice(Math.ceil(words.length / 2)).join(' ');
  const lower = (item.title + ' ' + (item.subtitle ?? '')).toLowerCase();

  let theme: HeroSlide['theme'] = 'shield';
  if (/reactor|iron/gi.test(lower)) theme = 'reactor';
  else if (/thor|martillo|mjolnir/gi.test(lower)) theme = 'mjolnir';
  else if (/infinito|gema/gi.test(lower)) theme = 'gems';

  return {
    id: item.id,
    kicker: item.subtitle?.trim() || 'Edición limitada:',
    titleLine1: line1 || 'AVENGERS',
    titleLine2: line2 || 'ASSEMBLE!',
    imageUrl: item.imageUrl,
    productRef: { name: item.title },
    theme,
    linkUrl: item.linkUrl
  };
}

@Component({
  selector: 'app-home-page',
  standalone: true,
  imports: [IconComponent, ProductCardComponent, ProductImageFallbackComponent, RouterLink, DecimalPipe, RevealDirective],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './home-page.html',
  styleUrls: ['./home-page.scss']
})
export class HomePageComponent {
  private readonly catalog = inject(CatalogService);
  private readonly cart = inject(CartService);

  readonly slides = signal<HeroSlide[]>(FALLBACK_SLIDES);
  readonly current = signal(0);

  readonly featured = signal<ProductListDto[]>(FALLBACK_PRODUCTS);
  readonly loading = signal(true);

  readonly currentSlide = computed(() => this.slides()[this.current()] ?? this.slides()[0]);

  constructor() {
    interval(8000)
      .pipe(takeUntilDestroyed())
      .subscribe(() => this.next());

    this.catalog.getSlider().subscribe((items) => {
      if (items.length > 0) {
        this.slides.set(items.map(toSlide));
        this.current.set(0);
      }
      this.loading.set(false);
    });

    this.catalog.getFeaturedProducts(4).subscribe((products) => {
      if (products.length > 0) {
        this.featured.set(products.slice(0, 4));
      }
      this.loading.set(false);
    });
  }

  protected next(): void {
    this.current.update((i) => (i + 1) % this.slides().length);
  }

  protected prev(): void {
    this.current.update((i) => (i - 1 + this.slides().length) % this.slides().length);
  }

  protected goTo(slide: HeroSlide): void {
    this.current.set(this.slides().findIndex((s) => s.id === slide.id));
  }

  protected addToCart(product: ProductListDto): void {
    this.cart.add(product);
  }
}