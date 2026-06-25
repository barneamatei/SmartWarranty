import { Component, computed, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { AuthViewModel } from '../../features/auth/view-models/auth.view-model';
import { NotificationsStateService } from '../../features/notifications/data/notifications-state.service';
import { PremiumAccessService } from '../auth/premium-access.service';

@Component({
  selector: 'app-shell',
  imports: [MatButtonModule, RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './app-shell.component.html',
  styleUrl: './app-shell.component.scss'
})
export class AppShellComponent {
  readonly authViewModel = inject(AuthViewModel);
  readonly notificationsState = inject(NotificationsStateService);
  readonly premiumAccess = inject(PremiumAccessService);
  readonly unreadNotifications = this.notificationsState.unreadCount.asReadonly();
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

  constructor() {
    const userId = this.authViewModel.user()?.userId || this.authViewModel.user()?.id;
    this.notificationsState.refreshUnreadCount(userId);
    this.premiumAccess.checkPremium().pipe(takeUntilDestroyed()).subscribe();
  }

  logout() {
    this.authViewModel.logout();
  }
}
