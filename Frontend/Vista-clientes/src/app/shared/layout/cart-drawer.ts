import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';

import { CartService, CartLine } from '../../core/services/cart.service';
import { CatalogService } from '../../core/services/catalog.service';
import { buildWhatsAppLink, resolveWhatsAppNumber } from '../../core/utils/whatsapp';
import { IconComponent } from '../icon/icon';
import { ProductImageFallbackComponent } from '../components/product-image-fallback/product-image-fallback';

@Component({
  selector: 'app-cart-drawer',
  standalone: true,
  imports: [IconComponent, RouterLink, ProductImageFallbackComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './cart-drawer.html',
  styleUrls: ['./cart-drawer.scss']
})
export class CartDrawerComponent {
  private readonly cart = inject(CartService);
  private readonly catalog = inject(CatalogService);

  readonly isOpen = this.cart.isOpen;
  readonly lines = this.cart.items;
  readonly subtotal = computed(() => this.format(this.cart.subtotal()));
  readonly isEmpty = computed(() => this.cart.count() === 0);

  protected updateQuantity(productId: string, quantity: number): void {
    if (quantity <= 0) {
      this.cart.remove(productId);
      return;
    }
    this.cart.setQuantity(productId, quantity);
  }

  protected remove(productId: string): void {
    this.cart.remove(productId);
  }

  protected clear(): void {
    this.cart.clear();
  }

  protected close(): void {
    this.cart.close();
  }

  protected linePrice(line: { product: { price: number }; quantity: number }): string {
    return this.format(line.product.price * line.quantity);
  }

  protected requestQuote(): void {
    const phone = resolveWhatsAppNumber(this.catalog.settings());
    if (!phone) return;

    const message = this.buildQuoteMessage(this.lines(), this.cart.subtotal());
    const link = buildWhatsAppLink(phone, message);
    window.open(link, '_blank', 'noopener');
  }

  private buildQuoteMessage(lines: CartLine[], subtotal: number): string {
    const parts = lines.map(
      (line) => `• ${line.product.name} x${line.quantity} — ${this.format(line.product.price * line.quantity)}`
    );
    return [
      'Hola, me gustaría una cotización de los siguientes artículos:',
      '',
      ...parts,
      '',
      `Subtotal: ${this.format(subtotal)}`
    ]
      .join('\n')
      .trim();
  }

  private format(value: number): string {
    return new Intl.NumberFormat('es-MX', { style: 'currency', currency: 'EUR' }).format(value);
  }
}