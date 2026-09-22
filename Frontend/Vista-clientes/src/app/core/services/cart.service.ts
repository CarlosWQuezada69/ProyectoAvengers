import { computed, Injectable, signal } from '@angular/core';

import { ProductListDto } from '../models/product';

export interface CartLine {
  product: ProductListDto;
  quantity: number;
}

const STORAGE_KEY = 'avengers-cart';

function loadCart(): CartLine[] {
  if (typeof localStorage === 'undefined') return [];

  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return [];
    const parsed = JSON.parse(raw) as CartLine[];
    return Array.isArray(parsed) ? parsed.filter((l) => l?.product?.id && l.quantity > 0) : [];
  } catch {
    return [];
  }
}

@Injectable({ providedIn: 'root' })
export class CartService {
  private readonly lines = signal<CartLine[]>(loadCart());

  readonly items = this.lines.asReadonly();
  readonly isOpen = signal(false);

  readonly count = computed(() => this.lines().reduce((acc, l) => acc + l.quantity, 0));
  readonly subtotal = computed(() => this.lines().reduce((acc, l) => acc + l.quantity * l.product.price, 0));

  add(product: ProductListDto, quantity = 1): void {
    const lines = this.lines();
    const index = lines.findIndex((l) => l.product.id === product.id);

    if (index >= 0) {
      const existing = lines[index];
      const nextQuantity = Math.min(existing.quantity + quantity, Math.max(product.stock, 1));
      this.lines.update((current) =>
        current.map((line, i) => (i === index ? { ...line, quantity: nextQuantity } : line))
      );
    } else {
      this.lines.update((current) => [
        ...current,
        { product, quantity: Math.min(quantity, Math.max(product.stock, 1)) }
      ]);
    }

    this.persist();
    this.isOpen.set(true);
  }

  setQuantity(productId: string, quantity: number): void {
    this.lines.update((current) =>
      current
        .map((line) =>
          line.product.id === productId ? { ...line, quantity: Math.max(1, quantity) } : line
        )
    );
    this.persist();
  }

  remove(productId: string): void {
    this.lines.update((current) => current.filter((line) => line.product.id !== productId));
    this.persist();
  }

  clear(): void {
    this.lines.set([]);
    this.persist();
  }

  open(): void {
    this.isOpen.set(true);
  }

  close(): void {
    this.isOpen.set(false);
  }

  private persist(): void {
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(this.lines()));
    } catch {
      // Storage no disponible: el carrito vive solo en memoria
    }
  }
}