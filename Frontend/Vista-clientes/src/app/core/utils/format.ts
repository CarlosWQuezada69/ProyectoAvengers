export const STORE_CURRENCY = 'DOP';
export const STORE_LOCALE = 'es-DO';

export function formatPrice(value: number): string {
  return new Intl.NumberFormat(STORE_LOCALE, {
    style: 'currency',
    currency: STORE_CURRENCY
  }).format(value);
}

export function formatDiscountPercent(price: number, compareAt: number | null | undefined): number {
  if (!compareAt || compareAt <= price) return 0;
  return Math.round(((compareAt - price) / compareAt) * 100);
}