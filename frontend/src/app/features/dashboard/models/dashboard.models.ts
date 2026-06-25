export interface UserProfileSummary {
  userId: string;
  name?: string;
  email?: string;
  status?: string;
}

export interface UserSummary {
  userId: string;
  email: string;
  status: string;
  userProfile?: UserProfileSummary | null;
}

export interface ProductSummary {
  productId: string;
  name: string;
  brand: string;
  model: string;
  categoryId: string;
  userId?: string | null;
  status: string;
}

export interface CategorySummary {
  categoryId: string;
  name: string;
  description: string;
  userId?: string | null;
}

export interface WarrantySummary {
  warrantyId: string;
  userId: string;
  productId: string;
  purchaseDate: string;
  expiryDate: string;
  durationMonths: number;
  status: string;
}

export interface NotificationSummary {
  notificationId: string;
  userId: string;
  title: string;
  message: string;
  type: string;
  channel: string;
  status: string;
  createdAt: string;
  readAt?: string | null;
}

export interface ClaimSummary {
  claimId: string;
  warrantyId: string;
  status: string;
  openedAt: string;
  closedAt?: string | null;
  description: string;
}

export interface ReportPreview {
  reportType: string;
  title: string;
  subtitle?: string;
  generatedAtUtc: string;
  recordCount: number;
  summary: Record<string, string>;
}

export interface DashboardSnapshot {
  users: UserSummary[];
  products: ProductSummary[];
  warranties: WarrantySummary[];
  notifications: NotificationSummary[];
  portfolioReport: ReportPreview | null;
  expiringReport: ReportPreview | null;
}
