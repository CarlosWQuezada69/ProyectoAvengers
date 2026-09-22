export interface CategoryDto {
  id: string;
  parentCategoryId: string | null;
  name: string;
  slug: string;
  description: string | null;
  imageUrl: string | null;
  isActive: boolean;
  displayOrder: number;
  children: CategoryDto[];
}