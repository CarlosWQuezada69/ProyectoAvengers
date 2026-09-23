export interface AboutInfo {
  id: string;
  title: string;
  history: string;
  mission: string | null;
  vision: string | null;
  createdAt: string;
  updatedAt: string | null;
  gallery: AboutGallery[];
}

export interface AboutGallery {
  id: string;
  url: string;
  altText: string | null;
  displayOrder: number;
  section: string;
}

export interface UpdateAboutInfoRequest {
  title: string;
  history: string;
  mission: string | null;
  vision: string | null;
}