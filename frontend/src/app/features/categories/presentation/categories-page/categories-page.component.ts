import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { AuthViewModel } from '../../../auth/view-models/auth.view-model';
import { DashboardApiService } from '../../../dashboard/data/dashboard-api.service';
import { CategorySummary } from '../../../dashboard/models/dashboard.models';
import { UiFeedbackService } from '../../../../shared/ui/ui-feedback.service';

@Component({
  selector: 'app-categories-page',
  imports: [MatButtonModule, MatCardModule, MatFormFieldModule, MatInputModule, MatProgressSpinnerModule, ReactiveFormsModule],
  templateUrl: './categories-page.component.html',
  styleUrl: './categories-page.component.scss'
})
export class CategoriesPageComponent {
  private readonly authViewModel = inject(AuthViewModel);
  private readonly dashboardApi = inject(DashboardApiService);
  private readonly formBuilder = inject(FormBuilder);
  private readonly uiFeedback = inject(UiFeedbackService);
  private readonly destroyRef = inject(DestroyRef);

  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly categories = signal<CategorySummary[]>([]);
  readonly editingCategoryId = signal<string | null>(null);
  readonly currentUserId = computed(() => this.authViewModel.user()?.userId || this.authViewModel.user()?.id || '');
  readonly categoriesCount = computed(() => this.categories().length);

  readonly createForm = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    description: ['', [Validators.maxLength(500)]]
  });

  readonly editForm = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    description: ['', [Validators.maxLength(500)]]
  });

  constructor() {
    this.loadCategories();
  }

  createCategory() {
    if (this.createForm.invalid || !this.currentUserId()) {
      this.createForm.markAllAsTouched();
      if (!this.currentUserId()) {
        this.uiFeedback.error('Trebuie sa fii autentificat ca sa creezi categorii.');
      }
      return;
    }

    this.saving.set(true);
    this.dashboardApi
      .createCategory({ ...this.createForm.getRawValue(), userId: this.currentUserId() })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.createForm.reset({ name: '', description: '' });
          this.uiFeedback.success('Categoria a fost creata.');
          this.loadCategories();
        },
        error: () => {
          this.saving.set(false);
          this.uiFeedback.error('Nu am putut crea categoria.');
        },
        complete: () => this.saving.set(false)
      });
  }

  startEdit(category: CategorySummary) {
    this.editingCategoryId.set(category.categoryId);
    this.editForm.setValue({
      name: category.name,
      description: category.description
    });
  }

  cancelEdit() {
    this.editingCategoryId.set(null);
    this.editForm.reset({ name: '', description: '' });
  }

  saveCategory(category: CategorySummary) {
    if (this.editForm.invalid) {
      this.editForm.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.dashboardApi
      .updateCategory(category.categoryId, this.editForm.getRawValue())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.cancelEdit();
          this.uiFeedback.success('Categoria a fost actualizata.');
          this.loadCategories();
        },
        error: () => {
          this.saving.set(false);
          this.uiFeedback.error('Nu am putut actualiza categoria.');
        },
        complete: () => this.saving.set(false)
      });
  }

  deleteCategory(category: CategorySummary) {
    this.uiFeedback
      .confirm('Stergere categorie', `Stergem categoria ${category.name}?`)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((confirmed) => {
        if (!confirmed) {
          return;
        }

        this.saving.set(true);
        this.dashboardApi
          .deleteCategory(category.categoryId)
          .pipe(takeUntilDestroyed(this.destroyRef))
          .subscribe({
            next: () => {
              this.uiFeedback.success('Categoria a fost stearsa.');
              this.loadCategories();
            },
            error: () => {
              this.saving.set(false);
              this.uiFeedback.error('Nu am putut sterge categoria.');
            },
            complete: () => this.saving.set(false)
          });
      });
  }

  private loadCategories() {
    this.loading.set(true);

    this.dashboardApi
      .getCategories(this.currentUserId())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (categories) => {
          this.categories.set(categories);
          this.loading.set(false);
        },
        error: () => {
          this.loading.set(false);
          this.uiFeedback.error('Nu am putut incarca categoriile.');
        }
      });
  }
}
