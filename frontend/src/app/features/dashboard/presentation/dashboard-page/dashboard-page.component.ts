import { DatePipe, NgClass } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { RouterLink } from '@angular/router';

import { PremiumAccessService } from '../../../../core/auth/premium-access.service';
import { apiConfig } from '../../../../core/config/api.config';
import { UiFeedbackService } from '../../../../shared/ui/ui-feedback.service';
import { AuthViewModel } from '../../../auth/view-models/auth.view-model';
import { DashboardApiService } from '../../data/dashboard-api.service';
import { DashboardSnapshot, NotificationSummary, WarrantySummary } from '../../models/dashboard.models';

@Component({
  selector: 'app-dashboard-page',
  imports: [DatePipe, NgClass, MatButtonModule, MatCardModule, MatProgressSpinnerModule, RouterLink],
  templateUrl: './dashboard-page.component.html',
  styleUrl: './dashboard-page.component.scss'
})
export class DashboardPageComponent {
  readonly authViewModel = inject(AuthViewModel);
  private readonly dashboardApi = inject(DashboardApiService);
  private readonly premiumAccess = inject(PremiumAccessService);
  private readonly uiFeedback = inject(UiFeedbackService);

  readonly now = new Date();
  readonly loading = signal(true);
  readonly snapshot = signal<DashboardSnapshot | null>(null);
  readonly isPremium = this.premiumAccess.isPremium;
  readonly isAdmin = computed(() => {
    const user = this.authViewModel.user();
    const roles = [user?.role, ...(user?.roles ?? [])].filter(Boolean).map((role) => role!.toLowerCase());
    return roles.includes('admin') || roles.includes('administrator');
  });

  readonly displayName = computed(() => {
    const user = this.authViewModel.user();
    const fullName = [user?.firstName, user?.lastName].filter(Boolean).join(' ').trim();
    return user?.fullName || user?.name || fullName || user?.email || 'utilizator';
  });

  readonly activeWarranties = computed(() =>
    this.snapshot()?.warranties.filter((warranty) => warranty.status.toLowerCase() === 'active').length ?? 0
  );

  readonly expiringWarranties = computed(() => {
    const today = new Date();
    const limit = new Date();
    limit.setDate(today.getDate() + 30);

    return (
      this.snapshot()?.warranties.filter((warranty) => {
        const expiry = new Date(warranty.expiryDate);
        return expiry >= today && expiry <= limit;
      }).length ?? 0
    );
  });

  readonly unreadNotifications = computed(() =>
    this.snapshot()?.notifications.filter((notification) => !notification.readAt).length ?? 0
  );

  readonly recentWarranties = computed(() =>
    [...(this.snapshot()?.warranties ?? [])]
      .sort((left, right) => new Date(right.expiryDate).getTime() - new Date(left.expiryDate).getTime())
      .slice(0, 5)
  );

  readonly recentNotifications = computed(() =>
    [...(this.snapshot()?.notifications ?? [])]
      .sort((left, right) => new Date(right.createdAt).getTime() - new Date(left.createdAt).getTime())
      .slice(0, 4)
  );

  constructor() {
    const userId = this.currentUserId();

    this.dashboardApi
      .getSnapshot(userId)
      .pipe(takeUntilDestroyed())
      .subscribe({
        next: (snapshot) => {
          this.snapshot.set(snapshot);
          this.loading.set(false);
        },
        error: () => {
          this.snapshot.set(null);
          this.loading.set(false);
        }
      });

    this.premiumAccess.checkPremium().pipe(takeUntilDestroyed()).subscribe();
  }

  logout() {
    this.authViewModel.logout();
  }

  statusClass(status: string) {
    return `status-${status.toLowerCase()}`;
  }

  warrantyLabel(warranty: WarrantySummary) {
    return `${warranty.warrantyId.slice(0, 8)} / ${warranty.status}`;
  }

  productLabel(productId: string) {
    const product = this.snapshot()?.products.find((item) => item.productId === productId);
    return product ? `${product.brand} ${product.name} ${product.model}`.trim() : 'Produs necunoscut';
  }

  notificationLabel(notification: NotificationSummary) {
    return `${notification.type} · ${notification.channel}`;
  }

  exportReport(report: 'portfolio' | 'expiring-warranties', format: 'pdf' | 'xlsx') {
    if (!this.isPremium()) {
      this.uiFeedback.error('Ai nevoie de cont Premium pentru rapoarte.');
      return;
    }

    const params = new URLSearchParams({ format });
    const userId = this.currentUserId();

    if (report === 'expiring-warranties') {
      params.set('daysAhead', '30');
    }

    if (userId) {
      params.set('userId', userId);
    }

    window.open(
      `${apiConfig.gatewayBaseUrl}/reports/api/reports/${report}/export?${params.toString()}`,
      '_blank',
      'noopener,noreferrer'
    );
  }

  private currentUserId() {
    return this.authViewModel.user()?.userId || this.authViewModel.user()?.id;
  }
}
