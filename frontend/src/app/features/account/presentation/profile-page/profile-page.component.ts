import { HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { RouterLink } from '@angular/router';

import { AuthViewModel } from '../../../auth/view-models/auth.view-model';
import { AuthApiService } from '../../../auth/data/auth-api.service';
import { AccountApiService } from '../../data/account-api.service';
import { UserRecord } from '../../models/account.models';
import { UiFeedbackService } from '../../../../shared/ui/ui-feedback.service';

const passwordStrengthPattern = /^(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$/;

const passwordMatchValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const newPassword = control.get('newPassword')?.value;
  const confirmNewPassword = control.get('confirmNewPassword')?.value;

  if (!newPassword || !confirmNewPassword) {
    return null;
  }

  return newPassword === confirmNewPassword ? null : { passwordMismatch: true };
};

@Component({
  selector: 'app-profile-page',
  imports: [
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    RouterLink,
    ReactiveFormsModule
  ],
  templateUrl: './profile-page.component.html',
  styleUrl: './profile-page.component.scss'
})
export class ProfilePageComponent {
  private readonly authViewModel = inject(AuthViewModel);
  private readonly authApi = inject(AuthApiService);
  private readonly accountApi = inject(AccountApiService);
  private readonly formBuilder = inject(FormBuilder);
  private readonly uiFeedback = inject(UiFeedbackService);
  private readonly destroyRef = inject(DestroyRef);

  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly changingPassword = signal(false);
  readonly userRecord = signal<UserRecord | null>(null);
  readonly profileMissing = signal(false);
  readonly currentUserId = computed(() => this.authViewModel.user()?.userId || this.authViewModel.user()?.id || '');
  readonly currentEmail = computed(() => this.authViewModel.user()?.email || '');

  readonly profileForm = this.formBuilder.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    name: ['', [Validators.required, Validators.maxLength(100)]],
    phone: ['', [Validators.maxLength(20)]],
    language: ['', [Validators.maxLength(10)]],
    preferences: ['', [Validators.maxLength(500)]]
  });

  readonly passwordForm = this.formBuilder.nonNullable.group(
    {
      currentPassword: ['', [Validators.required]],
      newPassword: ['', [Validators.required, Validators.pattern(passwordStrengthPattern)]],
      confirmNewPassword: ['', [Validators.required]]
    },
    { validators: [passwordMatchValidator] }
  );

  constructor() {
    this.loadProfile();
  }

  saveProfile() {
    if (this.profileForm.invalid || !this.currentUserId()) {
      this.profileForm.markAllAsTouched();
      return;
    }

    const payload = {
      email: this.profileForm.getRawValue().email,
      name: this.profileForm.getRawValue().name,
      phone: this.profileForm.getRawValue().phone || null,
      language: this.profileForm.getRawValue().language || null,
      preferences: this.profileForm.getRawValue().preferences || null
    };
    const isCreatingProfile = this.profileMissing();

    this.saving.set(true);

    const request$ = this.profileMissing()
      ? this.accountApi.createUser({
          userId: this.currentUserId(),
          ...payload
        })
      : this.accountApi.updateUser(this.currentUserId(), payload);

    request$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (user) => {
        this.userRecord.set(user);
        this.profileMissing.set(false);
        this.patchForm(user);
        this.uiFeedback.success(isCreatingProfile ? 'Profilul a fost creat.' : 'Profilul a fost actualizat.');
      },
      error: () => {
        this.uiFeedback.error('Nu am putut salva profilul.');
      },
      complete: () => this.saving.set(false)
    });
  }

  changePassword() {
    if (this.passwordForm.invalid) {
      this.passwordForm.markAllAsTouched();
      return;
    }

    this.changingPassword.set(true);

    this.authApi
      .changePassword(this.passwordForm.getRawValue())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.passwordForm.reset();
          this.uiFeedback.success('Parola a fost schimbata.');
        },
        error: (error: unknown) => {
          if (error instanceof HttpErrorResponse && error.error?.error) {
            this.uiFeedback.error(error.error.error);
            return;
          }

          this.uiFeedback.error('Nu am putut schimba parola.');
        },
        complete: () => this.changingPassword.set(false)
      });
  }

  private loadProfile() {
    if (!this.currentUserId()) {
      this.loading.set(false);
      this.profileForm.patchValue({ email: this.currentEmail() });
      return;
    }

    this.accountApi
      .getUserById(this.currentUserId())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (user) => {
          this.userRecord.set(user);
          this.profileMissing.set(false);
          this.patchForm(user);
          this.loading.set(false);
        },
        error: (error: unknown) => {
          if (error instanceof HttpErrorResponse && error.status === 404) {
            this.profileMissing.set(true);
            this.profileForm.patchValue({
              email: this.currentEmail(),
              name: this.authViewModel.user()?.fullName || this.authViewModel.user()?.name || '',
              phone: '',
              language: '',
              preferences: ''
            });
          } else {
            this.uiFeedback.error('Nu am putut incarca profilul utilizatorului.');
          }

          this.loading.set(false);
        }
      });
  }

  private patchForm(user: UserRecord) {
    this.profileForm.setValue({
      email: user.email,
      name: user.userProfile?.name || '',
      phone: user.userProfile?.phone || '',
      language: user.userProfile?.language || '',
      preferences: user.userProfile?.preferences || ''
    });
  }
}
