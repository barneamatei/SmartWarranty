import { HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';

import { AuthViewModel } from '../../../auth/view-models/auth.view-model';
import { AccountApiService } from '../../data/account-api.service';
import { SubscriptionRecord } from '../../models/account.models';
import { UiFeedbackService } from '../../../../shared/ui/ui-feedback.service';
import { PremiumAccessService } from '../../../../core/auth/premium-access.service';

@Component({
  selector: 'app-subscriptions-page',
  imports: [
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    ReactiveFormsModule
  ],
  templateUrl: './subscriptions-page.component.html',
  styleUrl: './subscriptions-page.component.scss'
})
export class SubscriptionsPageComponent {
  private readonly authViewModel = inject(AuthViewModel);
  private readonly accountApi = inject(AccountApiService);
  private readonly formBuilder = inject(FormBuilder);
  private readonly uiFeedback = inject(UiFeedbackService);
  private readonly premiumAccess = inject(PremiumAccessService);
  private readonly destroyRef = inject(DestroyRef);

  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly subscription = signal<SubscriptionRecord | null>(null);
  readonly subscriptionMissing = signal(false);
  readonly currentUserId = computed(() => this.authViewModel.user()?.userId || this.authViewModel.user()?.id || '');
  readonly planOptions = ['Free', 'Standard', 'Premium'];

  readonly subscriptionForm = this.formBuilder.nonNullable.group({
    planType: ['Free', [Validators.required]],
    startDate: ['', [Validators.required]],
    endDate: ['', [Validators.required]]
  });

  constructor() {
    this.loadSubscription();
  }

  saveSubscription() {
    if (this.subscriptionForm.invalid || !this.currentUserId()) {
      this.subscriptionForm.markAllAsTouched();
      return;
    }

    const value = this.subscriptionForm.getRawValue();
    this.saving.set(true);

    const request$ = this.subscriptionMissing()
      ? this.accountApi.createSubscription({
          userId: this.currentUserId(),
          planType: value.planType,
          startDate: value.startDate,
          endDate: value.endDate
        })
      : this.accountApi.updateSubscription(this.subscription()!.subscriptionId, {
          planType: value.planType,
          endDate: value.endDate
        });

    request$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (subscription) => {
        this.subscription.set(subscription);
        this.subscriptionMissing.set(false);
        this.patchForm(subscription);
        this.premiumAccess.refresh().pipe(takeUntilDestroyed(this.destroyRef)).subscribe();
        this.uiFeedback.success('Subscription actualizat.');
      },
      error: () => {
        this.uiFeedback.error('Nu am putut salva subscription-ul.');
      },
      complete: () => this.saving.set(false)
    });
  }

  deleteSubscription() {
    const currentSubscription = this.subscription();
    if (!currentSubscription) {
      return;
    }

    this.uiFeedback
      .confirm('Stergere subscription', `Stergem planul ${currentSubscription.planType}?`)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((confirmed) => {
        if (!confirmed) {
          return;
        }

        this.saving.set(true);
        this.accountApi
          .deleteSubscription(currentSubscription.subscriptionId)
          .pipe(takeUntilDestroyed(this.destroyRef))
          .subscribe({
            next: () => {
              this.subscription.set(null);
              this.subscriptionMissing.set(true);
              this.subscriptionForm.reset({
                planType: 'Free',
                startDate: this.today(),
                endDate: this.oneYearFromNow()
              });
              this.premiumAccess.refresh().pipe(takeUntilDestroyed(this.destroyRef)).subscribe();
              this.uiFeedback.success('Subscription sters.');
            },
            error: () => {
              this.uiFeedback.error('Nu am putut sterge subscription-ul.');
            },
            complete: () => this.saving.set(false)
          });
      });
  }

  private loadSubscription() {
    if (!this.currentUserId()) {
      this.loading.set(false);
      return;
    }

    this.accountApi
      .getSubscriptionByUserId(this.currentUserId())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (subscription) => {
          this.subscription.set(subscription);
          this.subscriptionMissing.set(false);
          this.patchForm(subscription);
          this.loading.set(false);
        },
        error: (error: unknown) => {
          if (error instanceof HttpErrorResponse && error.status === 404) {
            this.subscriptionMissing.set(true);
            this.subscriptionForm.reset({
              planType: 'Free',
              startDate: this.today(),
              endDate: this.oneYearFromNow()
            });
          } else {
            this.uiFeedback.error('Nu am putut incarca subscription-ul.');
          }

          this.loading.set(false);
        }
      });
  }

  private patchForm(subscription: SubscriptionRecord) {
    this.subscriptionForm.setValue({
      planType: subscription.planType,
      startDate: this.toInputDate(subscription.startDate),
      endDate: this.toInputDate(subscription.endDate)
    });
  }

  private today() {
    return new Date().toISOString().slice(0, 10);
  }

  private oneYearFromNow() {
    const nextYear = new Date();
    nextYear.setFullYear(nextYear.getFullYear() + 1);
    return nextYear.toISOString().slice(0, 10);
  }

  private toInputDate(value: string) {
    return new Date(value).toISOString().slice(0, 10);
  }
}
