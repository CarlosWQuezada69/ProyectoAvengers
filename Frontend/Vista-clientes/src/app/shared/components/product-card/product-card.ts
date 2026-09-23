import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { RouterLink } from '@angular/router';

import { ProductListDto } from '../../../core/models/product';
import { formatDiscountPercent, formatPrice } from '../../../core/utils/format';
import { IconComponent } from '../../icon/icon';
import { ProductImageFallbackComponent } from '../product-image-fallback/product-image-fallback';

@Component({
  selector: 'app-product-card',
  standalone: true,
  imports: [RouterLink, IconComponent, ProductImageFallbackComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './product-card.html',
  styleUrls: ['./product-card.scss']
})
export class ProductCardComponent {
  readonly product = input.required<ProductListDto>();

  readonly addRequested = output<ProductListDto>();

  readonly displayPrice = computed(() => formatPrice(this.product().price));
  readonly displayCompareAt = computed(() =>
    this.product().compareAtPrice ? formatPrice(this.product().compareAtPrice!) : null
  );
  readonly hasDiscount = computed(
    () => this.product().compareAtPrice != null && this.product().compareAtPrice! > this.product().price
  );
  readonly discountPercent = computed(() =>
    formatDiscountPercent(this.product().price, this.product().compareAtPrice)
  );
  readonly outOfStock = computed(() => this.product().stock <= 0);

  protected onAdd(): void {
    if (this.outOfStock()) return;
    this.addRequested.emit(this.product());
  }
}