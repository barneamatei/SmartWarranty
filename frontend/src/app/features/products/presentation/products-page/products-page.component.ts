import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { RouterLink } from '@angular/router';

import { AuthViewModel } from '../../../auth/view-models/auth.view-model';
import { DashboardApiService } from '../../../dashboard/data/dashboard-api.service';
import { CategorySummary, ProductSummary } from '../../../dashboard/models/dashboard.models';
import { UiFeedbackService } from '../../../../shared/ui/ui-feedback.service';

@Component({
  selector: 'app-products-page',
  imports: [
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    RouterLink,
    ReactiveFormsModule
  ],
  templateUrl: './products-page.component.html',
  styleUrl: './products-page.component.scss'
})
export class ProductsPageComponent {
  private readonly authViewModel = inject(AuthViewModel);
  private readonly dashboardApi = inject(DashboardApiService);
  private readonly formBuilder = inject(FormBuilder);
  private readonly uiFeedback = inject(UiFeedbackService);
  private readonly destroyRef = inject(DestroyRef);

  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly products = signal<ProductSummary[]>([]);
  readonly categories = signal<CategorySummary[]>([]);
  readonly editingProductId = signal<string | null>(null);
  readonly currentUserId = computed(() => this.authViewModel.user()?.userId || this.authViewModel.user()?.id || '');
  readonly activeProducts = computed(() => this.products().filter((product) => product.status.toLowerCase() === 'active').length);

  readonly createForm = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required]],
    brand: ['', [Validators.required]],
    model: ['', [Validators.required]],
    categoryId: ['', [Validators.required]]
  });

  readonly editForm = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required]],
    brand: ['', [Validators.required]],
    model: ['', [Validators.required]]
  });

  constructor() {
    this.loadProducts();
    this.loadCategories();
  }

  createProduct() {
    if (this.createForm.invalid || !this.currentUserId()) {
      this.createForm.markAllAsTouched();
      if (!this.currentUserId()) {
        this.uiFeedback.error('Trebuie sa fii autentificat ca sa adaugi produse.');
      }
      return;
    }

    this.saving.set(true);
    this.dashboardApi
      .createProduct({
        ...this.createForm.getRawValue(),
        userId: this.currentUserId()
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.createForm.reset({ name: '', brand: '', model: '', categoryId: '' });
          this.uiFeedback.success('Produsul a fost creat. Il poti folosi imediat cand creezi o garantie.');
          this.loadProducts();
        },
        error: () => {
          this.saving.set(false);
          this.uiFeedback.error('Nu am putut crea produsul.');
        },
        complete: () => this.saving.set(false)
      });
  }

  startEdit(product: ProductSummary) {
    if (!this.isOwnProduct(product)) {
      return;
    }

    this.editingProductId.set(product.productId);
    this.editForm.setValue({
      name: product.name,
      brand: product.brand,
      model: product.model
    });
  }

  cancelEdit() {
    this.editingProductId.set(null);
    this.editForm.reset();
  }

  saveProduct(product: ProductSummary) {
    if (this.editForm.invalid) {
      this.editForm.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.dashboardApi
      .updateProduct(product.productId, this.editForm.getRawValue())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.cancelEdit();
          this.uiFeedback.success('Produsul a fost actualizat.');
          this.loadProducts();
        },
        error: () => {
          this.saving.set(false);
          this.uiFeedback.error('Nu am putut actualiza produsul.');
        },
        complete: () => this.saving.set(false)
      });
  }

  deleteProduct(product: ProductSummary) {
    if (!this.isOwnProduct(product)) {
      return;
    }

    this.uiFeedback
      .confirm('Stergere produs', `Stergem produsul ${product.brand} ${product.name}?`)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((confirmed) => {
        if (!confirmed) {
          return;
        }

        this.saving.set(true);
        this.dashboardApi
          .deleteProduct(product.productId)
          .pipe(takeUntilDestroyed(this.destroyRef))
          .subscribe({
            next: () => {
              this.uiFeedback.success('Produsul a fost sters.');
              this.loadProducts();
            },
            error: () => {
              this.saving.set(false);
              this.uiFeedback.error('Nu am putut sterge produsul.');
            },
            complete: () => this.saving.set(false)
          });
      });
  }

  isOwnProduct(product: ProductSummary) {
    return !product.userId || product.userId === this.currentUserId();
  }

  private loadProducts() {
    this.loading.set(true);

    this.dashboardApi
      .getSnapshot(this.currentUserId())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (snapshot) => {
          this.products.set(snapshot.products);
          this.loading.set(false);
        },
        error: () => {
          this.loading.set(false);
          this.uiFeedback.error('Nu am putut incarca produsele.');
        }
      });
  }

  private loadCategories() {
    this.dashboardApi
      .getCategories(this.currentUserId())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((categories) => this.categories.set(categories));
  }
}
