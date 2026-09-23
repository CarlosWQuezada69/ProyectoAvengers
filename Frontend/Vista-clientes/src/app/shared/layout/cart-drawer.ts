import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';

import { CartService, CartLine } from '../../core/services/cart.service';
import { CatalogService } from '../../core/services/catalog.service';
import { buildWhatsAppLink, resolveWhatsAppNumber } from '../../core/utils/whatsapp';
import { formatPrice } from '../../core/utils/format';
import { IconComponent } from '../icon/icon';
import { AssetUrlPipe } from '../../core/pipes/asset-url.pipe';
import { ProductImageFallbackComponent } from '../components/product-image-fallback/product-image-fallback';

@Component({
  selector: 'app-cart-drawer',
  standalone: true,
  imports: [IconComponent, RouterLink, ProductImageFallbackComponent, AssetUrlPipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './cart-drawer.html',
  styleUrls: ['./cart-drawer.scss']
})
export class CartDrawerComponent {
  private readonly cart = inject(CartService);
  private readonly catalog = inject(CatalogService);

  readonly isOpen = this.cart.isOpen;
  readonly lines = this.cart.items;
  readonly subtotal = computed(() => formatPrice(this.cart.subtotal()));
  readonly isEmpty = computed(() => this.cart.count() === 0);

  protected readonly formatPrice = formatPrice;

  readonly businessName = computed(
    () => this.catalog.settings()?.business_name?.trim() || 'The Avengers Joyero'
  );
  readonly businessLogo = computed(() => this.catalog.settings()?.logo_url?.trim() || null);
  readonly rnc = computed(() => this.catalog.settings()?.rnc?.trim() ?? '');
  readonly address = computed(() => this.catalog.settings()?.address?.trim() ?? '');
  readonly contactEmail = computed(
    () => this.catalog.settings()?.contact_email ?? ''
  );
  readonly contactPhone = computed(
    () => this.catalog.settings()?.contact_phone ?? ''
  );
  readonly quoteDate = computed(() => {
    const d = new Date();
    const date = d.toLocaleDateString('es-DO', { day: '2-digit', month: '2-digit', year: 'numeric' });
    const time = d.toLocaleTimeString('es-DO', { hour: '2-digit', minute: '2-digit' });
    return `${date} ${time}`;
  });
  readonly quoteNumber = computed(() => {
    const d = new Date();
    const pad = (n: number) => String(n).padStart(2, '0');
    return `COT-${d.getFullYear()}${pad(d.getMonth() + 1)}${pad(d.getDate())}-${pad(d.getHours())}${pad(d.getMinutes())}`;
  });

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
    return formatPrice(line.product.price * line.quantity);
  }

  protected requestQuote(): void {
    const phone = resolveWhatsAppNumber(this.catalog.settings());
    if (!phone) return;

    const message = this.buildQuoteMessage(this.lines(), this.cart.subtotal());
    const link = buildWhatsAppLink(phone, message);
    window.open(link, '_blank', 'noopener');
  }

  protected printQuote(): void {
    this.cart.close();
    setTimeout(() => window.print(), 150);
  }

  private buildQuoteMessage(lines: CartLine[], subtotal: number): string {
    const parts = lines.map(
      (line) =>
        `• ${line.product.name} x${line.quantity} — ${formatPrice(line.product.price * line.quantity)}`
    );
    return [
      'Hola, me gustaría una cotización de los siguientes artículos:',
      '',
      ...parts,
      '',
      `Subtotal: ${formatPrice(subtotal)}`
    ]
      .join('\n')
      .trim();
  }
}