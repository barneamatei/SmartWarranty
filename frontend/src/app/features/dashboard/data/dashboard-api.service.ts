import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { catchError, forkJoin, map, of, switchMap } from 'rxjs';

import { apiConfig } from '../../../core/config/api.config';
import {
  CategorySummary,
  ClaimSummary,
  DashboardSnapshot,
  NotificationSummary,
  ProductSummary,
  ReportPreview,
  UserSummary,
  WarrantySummary
} from '../models/dashboard.models';

interface FamilyShareAccess {
  ownerUserId: string;
  memberUserId: string;
  permissions: number;
}

@Injectable({ providedIn: 'root' })
export class DashboardApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = apiConfig.gatewayBaseUrl;

  getSnapshot(userId?: string) {
    return this.getAccessibleUserIds(userId).pipe(
      switchMap((userIds) =>
        forkJoin({
          users: this.getUsers(),
          products: this.getProductsForUsers(userIds, userId),
          warranties: this.getWarrantiesForUsers(userIds, userId),
          notifications: userId ? this.getNotifications(userId) : of<NotificationSummary[]>([]),
          portfolioReport: this.getPortfolioReport(userId),
          expiringReport: this.getExpiringReport(userId)
        })
      )
    );
  }

  getCategories(userId?: string) {
    const query = userId ? `?userId=${userId}` : '';

    return this.http
      .get<CategorySummary[]>(`${this.baseUrl}/products/api/category${query}`)
      .pipe(catchError(() => of<CategorySummary[]>([])));
  }

  getProductsCatalog(userId?: string) {
    return this.getAccessibleUserIds(userId).pipe(
      switchMap((userIds) => this.getProductsForUsers(userIds, userId))
    );
  }

  createProduct(payload: { name: string; brand: string; model: string; categoryId: string; userId?: string }) {
    return this.http.post<ProductSummary>(`${this.baseUrl}/products/api/product`, payload);
  }

  createCategory(payload: { name: string; description: string; userId?: string }) {
    return this.http.post<CategorySummary>(`${this.baseUrl}/products/api/category`, payload);
  }

  updateCategory(categoryId: string, payload: { name: string; description: string }) {
    return this.http.put<CategorySummary>(`${this.baseUrl}/products/api/category/${categoryId}`, payload);
  }

  deleteCategory(categoryId: string) {
    return this.http.delete<void>(`${this.baseUrl}/products/api/category/${categoryId}`);
  }

  updateProduct(productId: string, payload: { name: string; brand: string; model: string }) {
    return this.http.put<ProductSummary>(`${this.baseUrl}/products/api/product/${productId}`, payload);
  }

  deleteProduct(productId: string) {
    return this.http.delete<void>(`${this.baseUrl}/products/api/product/${productId}`);
  }

  createWarranty(payload: {
    userId: string;
    productId: string;
    purchaseDate: string;
    durationMonths: number;
  }) {
    return this.http.post<WarrantySummary>(`${this.baseUrl}/warranties/api/warranty`, payload);
  }

  updateWarranty(
    warrantyId: string,
    payload: {
      userId: string;
      productId: string;
      purchaseDate: string;
      durationMonths: number;
      status?: string | null;
    }
  ) {
    return this.http.put<WarrantySummary>(`${this.baseUrl}/warranties/api/warranty/${warrantyId}`, payload);
  }

  deleteWarranty(warrantyId: string) {
    return this.http.delete<void>(`${this.baseUrl}/warranties/api/warranty/${warrantyId}`);
  }

  getClaimsByWarrantyId(warrantyId: string) {
    return this.http
      .get<ClaimSummary[]>(`${this.baseUrl}/warranties/api/claim/warranty/${warrantyId}`)
      .pipe(catchError(() => of<ClaimSummary[]>([])));
  }

  createClaim(payload: { warrantyId: string; description: string }) {
    return this.http.post<ClaimSummary>(`${this.baseUrl}/warranties/api/claim`, payload);
  }

  private getUsers() {
    return this.http
      .get<UserSummary[]>(`${this.baseUrl}/users/api/user`)
      .pipe(catchError(() => of<UserSummary[]>([])));
  }

  private getProducts(userId?: string) {
    return this.getProductsCatalog(userId);
  }

  private getWarranties(userId?: string) {
    return this.getAccessibleUserIds(userId).pipe(
      switchMap((userIds) => this.getWarrantiesForUsers(userIds, userId))
    );
  }

  private getNotifications(userId: string) {
    return this.http
      .get<NotificationSummary[]>(`${this.baseUrl}/notifications/api/notification/user/${userId}`)
      .pipe(catchError(() => of<NotificationSummary[]>([])));
  }

  getUnreadNotifications(userId: string) {
    return this.http
      .get<NotificationSummary[]>(`${this.baseUrl}/notifications/api/notification/user/${userId}/unread`)
      .pipe(catchError(() => of<NotificationSummary[]>([])));
  }

  markNotificationAsRead(notificationId: string) {
    return this.http.post<NotificationSummary>(`${this.baseUrl}/notifications/api/notification/${notificationId}/mark-read`, {});
  }

  private getPortfolioReport(userId?: string) {
    const query = userId ? `?userId=${userId}` : '';

    return this.http
      .get<ReportPreview>(`${this.baseUrl}/reports/api/reports/portfolio${query}`)
      .pipe(catchError(() => of<ReportPreview | null>(null)));
  }

  private getExpiringReport(userId?: string) {
    const query = userId ? `?daysAhead=30&userId=${userId}` : '?daysAhead=30';

    return this.http
      .get<ReportPreview>(`${this.baseUrl}/reports/api/reports/expiring-warranties${query}`)
      .pipe(catchError(() => of<ReportPreview | null>(null)));
  }

  private getAccessibleUserIds(userId?: string) {
    if (!userId) {
      return of<string[]>([]);
    }

    return this.http
      .get<FamilyShareAccess[]>(`${this.baseUrl}/users/api/familyshare/member/${userId}`)
      .pipe(
        map((shares) => this.uniqueIds([userId, ...shares.map((share) => share.ownerUserId)])),
        catchError(() => of([userId]))
      );
  }

  private getProductsForUsers(userIds: string[], requestedUserId?: string) {
    if (!requestedUserId) {
      return this.http
        .get<ProductSummary[]>(`${this.baseUrl}/products/api/product`)
        .pipe(catchError(() => of<ProductSummary[]>([])));
    }

    return this.getMergedItems(
      userIds.map((userId) => this.getProductsByUserId(userId)),
      (product) => product.productId
    );
  }

  private getWarrantiesForUsers(userIds: string[], requestedUserId?: string) {
    if (!requestedUserId) {
      return this.http
        .get<WarrantySummary[]>(`${this.baseUrl}/warranties/api/warranty`)
        .pipe(catchError(() => of<WarrantySummary[]>([])));
    }

    return this.getMergedItems(
      userIds.map((userId) => this.getWarrantiesByUserId(userId)),
      (warranty) => warranty.warrantyId
    );
  }

  private getProductsByUserId(userId: string) {
    return this.http
      .get<ProductSummary[]>(`${this.baseUrl}/products/api/product?userId=${userId}`)
      .pipe(catchError(() => of<ProductSummary[]>([])));
  }

  private getWarrantiesByUserId(userId: string) {
    return this.http
      .get<WarrantySummary[]>(`${this.baseUrl}/warranties/api/warranty/user/${userId}`)
      .pipe(catchError(() => of<WarrantySummary[]>([])));
  }

  private getMergedItems<T>(requests: ReturnType<typeof this.http.get<T[]>>[], getKey: (item: T) => string) {
    if (requests.length === 0) {
      return of<T[]>([]);
    }

    return forkJoin(requests).pipe(
      map((groups) => {
        const itemsById = new Map<string, T>();
        groups.flat().forEach((item) => itemsById.set(getKey(item), item));
        return Array.from(itemsById.values());
      })
    );
  }

  private uniqueIds(ids: string[]) {
    return Array.from(new Set(ids.filter(Boolean)));
  }
}
