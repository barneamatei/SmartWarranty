import { Injectable, inject, signal } from '@angular/core';
import { catchError, of } from 'rxjs';

import { DashboardApiService } from '../../dashboard/data/dashboard-api.service';

@Injectable({ providedIn: 'root' })
export class NotificationsStateService {
  private readonly dashboardApi = inject(DashboardApiService);

  readonly unreadCount = signal(0);

  refreshUnreadCount(userId?: string | null) {
    if (!userId) {
      this.unreadCount.set(0);
      return;
    }

    this.dashboardApi
      .getUnreadNotifications(userId)
      .pipe(
        catchError(() => {
          this.unreadCount.set(0);
          return of([]);
        })
      )
      .subscribe((notifications) => this.unreadCount.set(notifications.length));
  }
}
