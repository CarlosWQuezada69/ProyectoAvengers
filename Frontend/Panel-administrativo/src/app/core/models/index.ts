export interface SliderItem {
  id: string;
  title: string;
  subtitle?: string;
  imageUrl: string;
  linkUrl?: string;
  displayOrder: number;
  startsAt?: string;
  endsAt?: string;
  isActive: boolean;
}

export interface SiteSetting {
  id: string;
  key: string;
  value?: string;
}

export interface AuditLog {
  id: string;
  userId?: string;
  userName?: string;
  action: string;
  entityName: string;
  entityId?: string;
  changes?: Record<string, unknown>;
  ipAddress?: string;
  createdAt: string;
}

export interface StatsOverview {
  totalProducts: number;
  activeProducts: number;
  totalCategories: number;
  totalUsers: number;
  todayViews: number;
  todayPageViews: number;
  monthlyPageViews: number;
  lowStockCount: number;
  monthlyViews: number;
}

export interface DailyViewsStat {
  label: string;
  productViews: number;
  pageViews: number;
  total: number;
}

export interface PageViewsStat {
  pageKey: string;
  pageLabel: string;
  count: number;
}

export interface TopProduct {
  productId: string;
  productName: string;
  count: number;
}

export * from './about';
