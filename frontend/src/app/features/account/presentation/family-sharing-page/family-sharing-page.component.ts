import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { forkJoin } from 'rxjs';

import { AuthViewModel } from '../../../auth/view-models/auth.view-model';
import { AccountApiService } from '../../data/account-api.service';
import { FamilyShareRecord, UserRecord } from '../../models/account.models';
import { UiFeedbackService } from '../../../../shared/ui/ui-feedback.service';

@Component({
  selector: 'app-family-sharing-page',
  imports: [
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    ReactiveFormsModule
  ],
  templateUrl: './family-sharing-page.component.html',
  styleUrl: './family-sharing-page.component.scss'
})
export class FamilySharingPageComponent {
  private readonly authViewModel = inject(AuthViewModel);
  private readonly accountApi = inject(AccountApiService);
  private readonly formBuilder = inject(FormBuilder);
  private readonly uiFeedback = inject(UiFeedbackService);
  private readonly destroyRef = inject(DestroyRef);

  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly users = signal<UserRecord[]>([]);
  readonly ownedShares = signal<FamilyShareRecord[]>([]);
  readonly memberShares = signal<FamilyShareRecord[]>([]);
  readonly editingShareId = signal<string | null>(null);
  readonly currentUserId = computed(() => this.authViewModel.user()?.userId || this.authViewModel.user()?.id || '');
  readonly availableMembers = computed(() => this.users().filter((user) => user.userId !== this.currentUserId()));

  readonly createForm = this.formBuilder.nonNullable.group({
    memberUserId: ['', [Validators.required]],
    permissions: [1, [Validators.required, Validators.min(0)]]
  });

  readonly editForm = this.formBuilder.nonNullable.group({
    permissions: [1, [Validators.required, Validators.min(0)]]
  });

  constructor() {
    this.loadData();
  }

  createShare() {
    if (this.createForm.invalid || !this.currentUserId()) {
      this.createForm.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.accountApi
      .createFamilyShare({
        ownerUserId: this.currentUserId(),
        memberUserId: this.createForm.getRawValue().memberUserId,
        permissions: this.createForm.getRawValue().permissions
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.createForm.reset({ memberUserId: '', permissions: 1 });
          this.uiFeedback.success('Partajarea a fost creata.');
          this.loadData();
        },
        error: () => {
          this.uiFeedback.error('Nu am putut crea partajarea.');
        },
        complete: () => this.saving.set(false)
      });
  }

  startEdit(share: FamilyShareRecord) {
    this.editingShareId.set(share.shareId);
    this.editForm.setValue({ permissions: share.permissions });
  }

  cancelEdit() {
    this.editingShareId.set(null);
    this.editForm.reset({ permissions: 1 });
  }

  saveShare(share: FamilyShareRecord) {
    if (this.editForm.invalid) {
      this.editForm.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.accountApi
      .updateFamilyShare(share.shareId, this.editForm.getRawValue())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.cancelEdit();
          this.uiFeedback.success('Permisiunile au fost actualizate.');
          this.loadData();
        },
        error: () => {
          this.uiFeedback.error('Nu am putut actualiza permisiunile.');
        },
        complete: () => this.saving.set(false)
      });
  }

  deleteShare(share: FamilyShareRecord) {
    this.uiFeedback
      .confirm('Stergere partajare', `Eliminam accesul pentru ${this.userEmailLabel(share.memberUserId)}?`)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((confirmed) => {
        if (!confirmed) {
          return;
        }

        this.saving.set(true);
        this.accountApi
          .deleteFamilyShare(share.shareId)
          .pipe(takeUntilDestroyed(this.destroyRef))
          .subscribe({
            next: () => {
              this.uiFeedback.success('Partajarea a fost stearsa.');
              this.loadData();
            },
            error: () => {
              this.uiFeedback.error('Nu am putut sterge partajarea.');
            },
            complete: () => this.saving.set(false)
          });
      });
  }

  userLabel(userId: string) {
    const user = this.users().find((candidate) => candidate.userId === userId);
    return user?.userProfile?.name || userId;
  }

  userEmailLabel(userId: string) {
    const user = this.users().find((candidate) => candidate.userId === userId);
    return user?.email || userId;
  }

  private loadData() {
    if (!this.currentUserId()) {
      this.loading.set(false);
      return;
    }

    this.loading.set(true);
    forkJoin({
      users: this.accountApi.getAllUsers(),
      owned: this.accountApi.getOwnedShares(this.currentUserId()),
      member: this.accountApi.getMemberShares(this.currentUserId())
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: ({ users, owned, member }) => {
          this.users.set(users);
          this.ownedShares.set(owned);
          this.memberShares.set(member);
          this.loading.set(false);
        },
        error: () => {
          this.loading.set(false);
          this.uiFeedback.error('Nu am putut incarca datele de family sharing.');
        }
      });
  }
}
