import { DatePipe } from '@angular/common';
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

import { UiFeedbackService } from '../../../../shared/ui/ui-feedback.service';
import { AccountApiService } from '../../../account/data/account-api.service';
import { SubscriptionRecord, UserRecord } from '../../../account/models/account.models';

@Component({
  selector: 'app-admin-dashboard-page',
  imports: [
    DatePipe,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    ReactiveFormsModule
  ],
  templateUrl: './admin-dashboard-page.component.html',
  styleUrl: './admin-dashboard-page.component.scss'
})
export class AdminDashboardPageComponent {
  private readonly accountApi = inject(AccountApiService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly formBuilder = inject(FormBuilder);
  private readonly uiFeedback = inject(UiFeedbackService);

  readonly loading = signal(true);
  readonly savingUser = signal(false);
  readonly savingSubscription = signal(false);
  readonly users = signal<UserRecord[]>([]);
  readonly selectedUserId = signal<string | null>(null);
  readonly query = signal('');
  readonly subscriptionMissing = signal(false);
  readonly planOptions = ['Free', 'Standard', 'Premium'];

  readonly selectedUser = computed(
    () => this.users().find((user) => user.userId === this.selectedUserId()) ?? null
  );

  readonly filteredUsers = computed(() => {
    const query = this.query().trim().toLowerCase();
    if (!query) {
      return this.users();
    }

    return this.users().filter((user) => {
      const profile = user.userProfile;
      return [user.email, user.status, profile?.name, profile?.phone, user.subscription?.planType]
        .filter(Boolean)
        .some((value) => value!.toLowerCase().includes(query));
    });
  });

  readonly premiumUsers = computed(() => this.users().filter((user) => user.subscription?.isPremium).length);
  readonly activeUsers = computed(() => this.users().filter((user) => user.status.toLowerCase() === 'active').length);

  readonly userForm = this.formBuilder.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    name: ['', [Validators.required, Validators.maxLength(100)]],
    phone: [''],
    language: [''],
    preferences: ['']
  });

  readonly subscriptionForm = this.formBuilder.nonNullable.group({
    planType: ['Free', [Validators.required]],
    startDate: ['', [Validators.required]],
    endDate: ['', [Validators.required]]
  });

  constructor() {
    this.loadUsers();
  }

  selectUser(userId: string) {
    this.selectedUserId.set(userId);
    const user = this.selectedUser();
    if (!user) {
      return;
    }

    this.patchUserForm(user);
    this.loadSubscription(user.userId);
  }

  updateQuery(value: string) {
    this.query.set(value);
  }

  saveUser() {
    const user = this.selectedUser();
    if (!user || this.userForm.invalid) {
      this.userForm.markAllAsTouched();
      return;
    }

    const value = this.userForm.getRawValue();
    this.savingUser.set(true);

    this.accountApi
      .updateUser(user.userId, {
        email: value.email,
        name: value.name,
        phone: value.phone || null,
        language: value.language || null,
        preferences: value.preferences || null
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (updatedUser) => {
          this.users.update((items) => items.map((item) => (item.userId === updatedUser.userId ? updatedUser : item)));
          this.patchUserForm(updatedUser);
          this.uiFeedback.success('User actualizat.');
        },
        error: () => this.uiFeedback.error('Nu am putut actualiza userul.'),
        complete: () => this.savingUser.set(false)
      });
  }

  saveSubscription() {
    const user = this.selectedUser();
    if (!user || this.subscriptionForm.invalid) {
      this.subscriptionForm.markAllAsTouched();
      return;
    }

    const value = this.subscriptionForm.getRawValue();
    this.savingSubscription.set(true);

    const request$ = this.subscriptionMissing() || !user.subscription
      ? this.accountApi.createSubscription({
          userId: user.userId,
          planType: value.planType,
          startDate: value.startDate,
          endDate: value.endDate
        })
      : this.accountApi.updateSubscription(user.subscription.subscriptionId, {
          planType: value.planType,
          endDate: value.endDate
        });

    request$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (subscription) => {
        this.applySubscription(subscription);
        this.subscriptionMissing.set(false);
        this.patchSubscriptionForm(subscription);
        this.uiFeedback.success('Subscription actualizat.');
      },
      error: () => this.uiFeedback.error('Nu am putut salva subscription-ul.'),
      complete: () => this.savingSubscription.set(false)
    });
  }

  deleteSubscription() {
    const subscription = this.selectedUser()?.subscription;
    if (!subscription) {
      return;
    }

    this.uiFeedback
      .confirm('Stergere subscription', `Stergem planul ${subscription.planType} pentru userul selectat?`)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((confirmed) => {
        if (!confirmed) {
          return;
        }

        this.savingSubscription.set(true);
        this.accountApi
          .deleteSubscription(subscription.subscriptionId)
          .pipe(takeUntilDestroyed(this.destroyRef))
          .subscribe({
            next: () => {
              this.users.update((items) =>
                items.map((user) => (user.userId === subscription.userId ? { ...user, subscription: null } : user))
              );
              this.subscriptionMissing.set(true);
              this.subscriptionForm.reset({
                planType: 'Free',
                startDate: this.today(),
                endDate: this.oneYearFromNow()
              });
              this.uiFeedback.success('Subscription sters.');
            },
            error: () => this.uiFeedback.error('Nu am putut sterge subscription-ul.'),
            complete: () => this.savingSubscription.set(false)
          });
      });
  }

  private loadUsers() {
    this.loading.set(true);

    this.accountApi
      .getAllUsers()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (users) => {
          const sortedUsers = [...users].sort((left, right) => left.email.localeCompare(right.email));
          this.users.set(sortedUsers);

          if (sortedUsers.length > 0) {
            this.selectUser(sortedUsers[0].userId);
          }

          this.loading.set(false);
        },
        error: () => {
          this.uiFeedback.error('Nu am putut incarca userii.');
          this.loading.set(false);
        }
      });
  }

  private loadSubscription(userId: string) {
    const user = this.selectedUser();
    if (user?.subscription) {
      this.subscriptionMissing.set(false);
      this.patchSubscriptionForm(user.subscription);
      return;
    }

    this.accountApi
      .getSubscriptionByUserId(userId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (subscription) => {
          this.applySubscription(subscription);
          this.subscriptionMissing.set(false);
          this.patchSubscriptionForm(subscription);
        },
        error: (error: unknown) => {
          if (!(error instanceof HttpErrorResponse) || error.status !== 404) {
            this.uiFeedback.error('Nu am putut incarca subscription-ul userului.');
          }

          this.subscriptionMissing.set(true);
          this.subscriptionForm.reset({
            planType: 'Free',
            startDate: this.today(),
            endDate: this.oneYearFromNow()
          });
        }
      });
  }

  private applySubscription(subscription: SubscriptionRecord) {
    this.users.update((items) =>
      items.map((user) => (user.userId === subscription.userId ? { ...user, subscription } : user))
    );
  }

  private patchUserForm(user: UserRecord) {
    this.userForm.setValue({
      email: user.email,
      name: user.userProfile?.name ?? '',
      phone: user.userProfile?.phone ?? '',
      language: user.userProfile?.language ?? '',
      preferences: user.userProfile?.preferences ?? ''
    });
  }

  private patchSubscriptionForm(subscription: SubscriptionRecord) {
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
