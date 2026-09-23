export interface AboutGalleryDto {
  id: string;
  url: string;
  altText: string | null;
  displayOrder: number;
  section: string;
}

export interface AboutInfoDto {
  id?: string;
  title: string;
  history: string;
  mission: string | null;
  vision: string | null;
  createdAt?: string;
  updatedAt?: string | null;
  gallery: AboutGalleryDto[];
}