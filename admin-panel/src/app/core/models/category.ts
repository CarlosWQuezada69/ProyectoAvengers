export interface Category {
  id: string;
  parentCategoryId?: string;
  name: string;
  slug: string;
  description?: string;
  imageUrl?: string;
  isActive: boolean;
  displayOrder: number;
  children?: Category[];
}
