import { DatePipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { UiFeedbackService } from '../../../../shared/ui/ui-feedback.service';
import { AuthViewModel } from '../../../auth/view-models/auth.view-model';
import { DashboardApiService } from '../../../dashboard/data/dashboard-api.service';
import { NotificationSummary } from '../../../dashboard/models/dashboard.models';
import { NotificationsStateService } from '../../data/notifications-state.service';

@Component({
  selector: 'app-notifications-page',
  imports: [DatePipe, MatButtonModule, MatCardModule, MatProgressSpinnerModule],
  templateUrl: './notifications-page.component.html',
  styleUrl: './notifications-page.component.scss'
})
export class NotificationsPageComponent {
  private readonly authViewModel = inject(AuthViewModel);
  private readonly dashboardApi = inject(DashboardApiService);
  private readonly notificationsState = inject(NotificationsStateService);
  private readonly uiFeedback = inject(UiFeedbackService);
  readonly loading = signal(true);
  readonly pendingReadIds = signal<Record<string, boolean>>({});
  readonly notifications = signal<NotificationSummary[]>([]);
  readonly filter = signal<'all' | 'unread'>('all');
  readonly unreadCount = computed(() => this.notifications().filter((notification) => !notification.readAt).length);
  readonly visibleNotifications = computed(() =>
    this.filter() === 'unread' ? this.notifications().filter((notification) => !notification.readAt) : this.notifications()
  );

  constructor() {
    this.loadNotifications();
  }

  setFilter(filter: 'all' | 'unread') {
    this.filter.set(filter);
  }

  markAsRead(notificationId: string) {
    this.pendingReadIds.update((state) => ({ ...state, [notificationId]: true }));
    this.dashboardApi
      .markNotificationAsRead(notificationId)
      .pipe(takeUntilDestroyed())
      .subscribe({
        next: (updated) => {
          this.notifications.update((items) =>
            items.map((notification) => (notification.notificationId === notificationId ? updated : notification))
          );
          this.syncUnreadCount();
        },
        error: () => {
          this.uiFeedback.error('Nu am putut marca notificarea ca citita.');
        },
        complete: () => {
          this.pendingReadIds.update((state) => {
            const nextState = { ...state };
            delete nextState[notificationId];
            return nextState;
          });
        }
      });
  }

  private loadNotifications() {
    const userId = this.authViewModel.user()?.userId || this.authViewModel.user()?.id;

    this.dashboardApi
      .getSnapshot(userId)
      .pipe(takeUntilDestroyed())
      .subscribe({
        next: (snapshot) => {
          this.notifications.set(snapshot.notifications);
          this.syncUnreadCount();
          this.loading.set(false);
        },
        error: () => {
          this.loading.set(false);
          this.uiFeedback.error('Nu am putut incarca notificarile.');
        }
      });
  }

  private syncUnreadCount() {
    this.notificationsState.unreadCount.set(this.unreadCount());
  }
}
