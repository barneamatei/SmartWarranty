import { DatePipe, NgClass } from '@angular/common';
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
import { DashboardApiService } from '../../../dashboard/data/dashboard-api.service';
import { ClaimSummary, ProductSummary, WarrantySummary } from '../../../dashboard/models/dashboard.models';
import { UiFeedbackService } from '../../../../shared/ui/ui-feedback.service';
import { forkJoin } from 'rxjs';

@Component({
  selector: 'app-warranties-page',
  imports: [
    DatePipe,
    NgClass,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    ReactiveFormsModule
  ],
  templateUrl: './warranties-page.component.html',
  styleUrl: './warranties-page.component.scss'
})
export class WarrantiesPageComponent {
  private readonly authViewModel = inject(AuthViewModel);
  private readonly dashboardApi = inject(DashboardApiService);
  private readonly formBuilder = inject(FormBuilder);
  private readonly uiFeedback = inject(UiFeedbackService);
  private readonly destroyRef = inject(DestroyRef);

  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly warranties = signal<WarrantySummary[]>([]);
  readonly products = signal<ProductSummary[]>([]);
  readonly claimsByWarrantyId = signal<Record<string, ClaimSummary[]>>({});
  readonly loadingClaims = signal<Record<string, boolean>>({});
  readonly editingWarrantyId = signal<string | null>(null);
  readonly claimFormWarrantyId = signal<string | null>(null);
  readonly activeCount = computed(() => this.warranties().filter((warranty) => warranty.status.toLowerCase() === 'active').length);
  readonly claimedCount = computed(() => this.warranties().filter((warranty) => warranty.status.toLowerCase() === 'claimed').length);
  readonly userId = computed(() => this.authViewModel.user()?.userId || this.authViewModel.user()?.id || '');

  readonly createForm = this.formBuilder.nonNullable.group({
    productId: ['', [Validators.required]],
    purchaseDate: ['', [Validators.required]],
    durationMonths: [24, [Validators.required, Validators.min(1)]]
  });

  readonly editForm = this.formBuilder.nonNullable.group({
    productId: ['', [Validators.required]],
    purchaseDate: ['', [Validators.required]],
    durationMonths: [12, [Validators.required, Validators.min(1)]],
    status: ['Active', [Validators.required]]
  });

  readonly claimForm = this.formBuilder.nonNullable.group({
    description: ['', [Validators.required, Validators.minLength(10)]]
  });

  readonly statuses = ['Active', 'Expired', 'Claimed', 'Inactive'];

  constructor() {
    this.loadWarranties();
  }

  statusClass(status: string) {
    return `status-${status.toLowerCase()}`;
  }

  productLabel(productId: string) {
    const product = this.products().find((item) => item.productId === productId);
    return product ? `${product.brand} ${product.name} ${product.model}`.trim() : 'Produs necunoscut';
  }

  createWarranty() {
    if (this.createForm.invalid || !this.userId()) {
      this.createForm.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.dashboardApi
      .createWarranty({
        userId: this.userId(),
        productId: this.createForm.getRawValue().productId,
        purchaseDate: this.createForm.getRawValue().purchaseDate,
        durationMonths: this.createForm.getRawValue().durationMonths
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.createForm.reset({
            productId: '',
            purchaseDate: '',
            durationMonths: 24
          });
          this.uiFeedback.success('Garantia a fost creata.');
          this.loadWarranties();
        },
        error: () => {
          this.saving.set(false);
          this.uiFeedback.error('Nu am putut crea garantia.');
        },
        complete: () => this.saving.set(false)
      });
  }

  startEdit(warranty: WarrantySummary) {
    if (!this.isOwnWarranty(warranty)) {
      return;
    }

    this.editingWarrantyId.set(warranty.warrantyId);
    this.editForm.setValue({
      productId: warranty.productId,
      purchaseDate: this.toInputDate(warranty.purchaseDate),
      durationMonths: warranty.durationMonths,
      status: warranty.status
    });
  }

  cancelEdit() {
    this.editingWarrantyId.set(null);
    this.editForm.reset({ productId: '', purchaseDate: '', durationMonths: 12, status: 'Active' });
  }

  saveWarranty(warranty: WarrantySummary) {
    if (this.editForm.invalid || !this.userId()) {
      this.editForm.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.dashboardApi
      .updateWarranty(warranty.warrantyId, {
        userId: this.userId(),
        productId: this.editForm.getRawValue().productId,
        purchaseDate: this.editForm.getRawValue().purchaseDate,
        durationMonths: this.editForm.getRawValue().durationMonths,
        status: this.editForm.getRawValue().status
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.cancelEdit();
          this.uiFeedback.success('Garantia a fost actualizata.');
          this.loadWarranties();
        },
        error: () => {
          this.saving.set(false);
          this.uiFeedback.error('Nu am putut actualiza garantia.');
        },
        complete: () => this.saving.set(false)
      });
  }

  deleteWarranty(warranty: WarrantySummary) {
    if (!this.isOwnWarranty(warranty)) {
      return;
    }

    this.uiFeedback
      .confirm('Stergere garantie', `Stergem garantia pentru ${this.productLabel(warranty.productId)}?`)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((confirmed) => {
        if (!confirmed) {
          return;
        }

        this.saving.set(true);
        this.dashboardApi
          .deleteWarranty(warranty.warrantyId)
          .pipe(takeUntilDestroyed(this.destroyRef))
          .subscribe({
            next: () => {
              this.uiFeedback.success('Garantia a fost stearsa.');
              this.loadWarranties();
            },
            error: () => {
              this.saving.set(false);
              this.uiFeedback.error('Nu am putut sterge garantia.');
            },
            complete: () => this.saving.set(false)
          });
      });
  }

  openClaimForm(warrantyId: string) {
    const warranty = this.warranties().find((item) => item.warrantyId === warrantyId);
    if (warranty && !this.isOwnWarranty(warranty)) {
      return;
    }

    this.claimFormWarrantyId.set(warrantyId);
    this.claimForm.reset({ description: '' });
    this.loadClaims(warrantyId);
  }

  closeClaimForm() {
    this.claimFormWarrantyId.set(null);
    this.claimForm.reset({ description: '' });
  }

  claimsForWarranty(warrantyId: string) {
    return this.claimsByWarrantyId()[warrantyId] ?? [];
  }

  isOwnWarranty(warranty: WarrantySummary) {
    return warranty.userId === this.userId();
  }

  createClaim(warranty: WarrantySummary) {
    if (!this.isOwnWarranty(warranty)) {
      return;
    }

    if (this.claimForm.invalid) {
      this.claimForm.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.dashboardApi
      .createClaim({
        warrantyId: warranty.warrantyId,
        description: this.claimForm.getRawValue().description
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.uiFeedback.success('Claim-ul a fost trimis.');
          this.claimForm.reset({ description: '' });
          this.loadClaims(warranty.warrantyId);
          this.loadWarranties();
        },
        error: () => {
          this.saving.set(false);
          this.uiFeedback.error('Nu am putut crea claim-ul.');
        },
        complete: () => this.saving.set(false)
      });
  }

  private loadWarranties() {
    this.loading.set(true);

    forkJoin({
      snapshot: this.dashboardApi.getSnapshot(this.userId()),
      catalogProducts: this.dashboardApi.getProductsCatalog(this.userId())
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: ({ snapshot, catalogProducts }) => {
          this.warranties.set(snapshot.warranties);
          this.products.set(catalogProducts);
          this.claimsByWarrantyId.set({});
          this.loading.set(false);
        },
        error: () => {
          this.loading.set(false);
          this.uiFeedback.error('Nu am putut incarca garantiile.');
        }
      });
  }

  private loadClaims(warrantyId: string) {
    this.loadingClaims.update((state) => ({ ...state, [warrantyId]: true }));

    this.dashboardApi
      .getClaimsByWarrantyId(warrantyId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (claims) => {
          this.claimsByWarrantyId.update((state) => ({ ...state, [warrantyId]: claims }));
          this.loadingClaims.update((state) => ({ ...state, [warrantyId]: false }));
        },
        error: () => {
          this.loadingClaims.update((state) => ({ ...state, [warrantyId]: false }));
          this.uiFeedback.error('Nu am putut incarca claim-urile pentru aceasta garantie.');
        }
      });
  }

  private toInputDate(value: string) {
    return new Date(value).toISOString().slice(0, 10);
  }
}
