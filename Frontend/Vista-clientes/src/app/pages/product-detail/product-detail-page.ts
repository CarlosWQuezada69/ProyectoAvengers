import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { CatalogService } from '../../core/services/catalog.service';
import { CartService } from '../../core/services/cart.service';
import { ProductDto, ProductListDto } from '../../core/models/product';
import { IconComponent } from '../../shared/icon/icon';
import { ProductImageFallbackComponent } from '../../shared/components/product-image-fallback/product-image-fallback';
import { formatPrice } from '../../core/utils/format';

@Component({
  selector: 'app-product-detail-page',
  standalone: true,
  imports: [IconComponent, RouterLink, ProductImageFallbackComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './product-detail-page.html',
  styleUrls: ['./product-detail-page.scss']
})
export class ProductDetailPageComponent {
  private readonly catalog = inject(CatalogService);
  private readonly cart = inject(CartService);
  private readonly route = inject(ActivatedRoute);

  readonly product = signal<ProductDto | null>(null);
  readonly loading = signal(true);

  readonly selectedImageIndex = signal(0);
  readonly quantity = signal(1);

  readonly price = computed(() => (this.product() ? formatPrice(this.product()!.price) : ''));
  readonly compareAt = computed(() =>
    this.product()?.compareAtPrice ? formatPrice(this.product()!.compareAtPrice!) : null
  );
  readonly hasDiscount = computed(
    () => (this.product()?.compareAtPrice ?? 0) > (this.product()?.price ?? 0)
  );
  readonly inStock = computed(() => (this.product()?.stock ?? 0) > 0);
  readonly lowStock = computed(() => (this.product()?.stock ?? 0) > 0 && this.product()!.stock <= 5);
  readonly stockLabel = computed(() => {
    const stock = this.product()?.stock ?? 0;
    if (stock <= 0) return 'Agotado';
    if (stock <= 5) return `Últimas unidades (${stock})`;
    return 'En stock';
  });
  readonly maxQuantity = computed(() => Math.max(this.product()?.stock ?? 1, 1));

  protected readonly gallery = computed(() => ({
    image: this.product()?.images[this.selectedImageIndex()] ?? null,
    count: this.product()?.images.length ?? 0
  }));

  constructor() {
    this.route.paramMap.pipe(takeUntilDestroyed()).subscribe((params) => {
      const slug = params.get('slug');
      if (slug) this.load(slug);
    });
  }

  private load(slug: string): void {
    this.loading.set(true);
    this.product.set(null);
    this.quantity.set(1);
    this.selectedImageIndex.set(0);

    this.catalog.getProduct(slug).subscribe({
      next: (product) => {
        this.product.set(product);
        this.loading.set(false);
        this.catalog.trackView(product.id).subscribe();
      },
      error: () => {
        this.product.set(null);
        this.loading.set(false);
      }
    });
  }

  protected addToCart(): void {
    const product = this.product();
    if (!product || product.stock <= 0) return;

    this.cart.add(
      {
        id: product.id,
        sku: product.sku,
        name: product.name,
        slug: product.slug,
        price: product.price,
        compareAtPrice: product.compareAtPrice,
        stock: product.stock,
        categoryName: product.categoryName,
        isActive: product.isActive,
        isFeatured: product.isFeatured,
        primaryImageUrl: product.images.find((i) => i.isPrimary)?.url ?? product.images[0]?.url ?? null,
        createdAt: product.createdAt
      } satisfies ProductListDto,
      this.quantity()
    );
  }

  protected changeQuantity(delta: number): void {
    this.quantity.update((q) => Math.min(Math.max(q + delta, 1), this.maxQuantity()));
  }

  protected selectImage(index: number): void {
    this.selectedImageIndex.set(index);
  }

  protected restrictionLabel(type: string): string {
    switch (type) {
      case 'AGE_MIN':
        return 'Requiere ser mayor de edad';
      case 'PURCHASE_LIMIT_USER':
        return 'Compra limitada por cliente';
      case 'PURCHASE_LIMIT_ORDER':
        return 'Cantidad máxima por pedido';
      case 'AVAILABILITY_WINDOW':
        return 'Disponible solo por tiempo limitado';
      case 'GEOGRAPHIC':
        return 'Venta con restricciones geográficas';
      case 'LIMITED_STOCK':
        return 'Edición limitada — stock reducido';
      default:
        return 'Aplican condiciones especiales';
    }
  }
}