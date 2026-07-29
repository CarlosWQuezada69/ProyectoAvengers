export interface Product {
  id: string;
  sku: string;
  name: string;
  slug: string;
  description?: string;
  price: number;
  compareAtPrice?: number;
  stock: number;
  categoryId?: string;
  isActive: boolean;
  isFeatured: boolean;
  categoryName?: string;
  primaryImageUrl?: string;
  images: ProductImage[];
  restrictions: ProductRestriction[];
  rowVersion?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface ProductImage {
  id: string;
  url: string;
  altText?: string;
  displayOrder: number;
  isPrimary: boolean;
}

export interface ProductRestriction {
  id: string;
  productId: string;
  restrictionType: RestrictionType;
  config: Record<string, unknown>;
  startsAt?: string;
  endsAt?: string;
  isActive: boolean;
}

export type RestrictionType =
  | 'AGE_MIN'
  | 'PURCHASE_LIMIT_USER'
  | 'PURCHASE_LIMIT_ORDER'
  | 'AVAILABILITY_WINDOW'
  | 'GEOGRAPHIC'
  | 'LIMITED_STOCK';
