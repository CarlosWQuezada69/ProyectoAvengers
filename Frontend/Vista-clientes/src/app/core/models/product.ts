export interface ProductListDto {
  id: string;
  sku: string;
  name: string;
  slug: string;
  price: number;
  compareAtPrice: number | null;
  stock: number;
  categoryName: string | null;
  isActive: boolean;
  isFeatured: boolean;
  primaryImageUrl: string | null;
  createdAt: string;
}

export interface ProductImageDto {
  id: string;
  url: string;
  altText: string | null;
  displayOrder: number;
  isPrimary: boolean;
}

export interface ProductRestrictionDto {
  id: string;
  restrictionType: string;
  config: string;
  startsAt: string | null;
  endsAt: string | null;
  isActive: boolean;
}

export interface ProductDto {
  id: string;
  sku: string;
  name: string;
  slug: string;
  description: string | null;
  price: number;
  compareAtPrice: number | null;
  stock: number;
  categoryId: string | null;
  categoryName: string | null;
  isActive: boolean;
  isFeatured: boolean;
  createdAt: string;
  updatedAt: string | null;
  images: ProductImageDto[];
  restrictions: ProductRestrictionDto[];
  structuredData: string | null;
}

export interface ProductQuery {
  search?: string | null;
  categoryId?: string | null;
  minPrice?: number | null;
  maxPrice?: number | null;
  onlyAvailable?: boolean | null;
  sort?: string | null;
  page?: number;
  pageSize?: number;
}