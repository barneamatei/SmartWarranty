import { Injectable, computed, inject, signal } from '@angular/core';
import { catchError, map, of, tap } from 'rxjs';

import { AccountApiService } from '../../features/account/data/account-api.service';
import { SubscriptionRecord } from '../../features/account/models/account.models';
import { AuthViewModel } from '../../features/auth/view-models/auth.view-model';

@Injectable({ providedIn: 'root' })
export class PremiumAccessService {
  private readonly accountApi = inject(AccountApiService);
  private readonly authViewModel = inject(AuthViewModel);

  private readonly premium = signal(false);
  private readonly checkedUserId = signal<string | null>(null);

  readonly isPremium = this.premium.asReadonly();
  readonly hasCheckedCurrentUser = computed(() => this.checkedUserId() === this.currentUserId());

  checkPremium() {
    const userId = this.currentUserId();
    if (!userId) {
      this.checkedUserId.set(null);
      this.premium.set(false);
      return of(false);
    }

    if (this.checkedUserId() === userId) {
      return of(this.premium());
    }

    return this.accountApi.getSubscriptionByUserId(userId).pipe(
      map((subscription) => this.isActivePremium(subscription)),
      tap((isPremium) => {
        this.checkedUserId.set(userId);
        this.premium.set(isPremium);
      }),
      catchError(() => {
        this.checkedUserId.set(userId);
        this.premium.set(false);
        return of(false);
      })
    );
  }

  refresh() {
    this.checkedUserId.set(null);
    return this.checkPremium();
  }

  private currentUserId() {
    return this.authViewModel.user()?.userId || this.authViewModel.user()?.id || '';
  }

  private isActivePremium(subscription: SubscriptionRecord) {
    if (!subscription.isPremium) {
      return false;
    }

    const endDate = new Date(subscription.endDate);
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    return Number.isNaN(endDate.getTime()) || endDate >= today;
  }
}
