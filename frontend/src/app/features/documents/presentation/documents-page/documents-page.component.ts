import { DatePipe } from '@angular/common';
import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { forkJoin } from 'rxjs';

import { UiFeedbackService } from '../../../../shared/ui/ui-feedback.service';
import { AuthViewModel } from '../../../auth/view-models/auth.view-model';
import { DashboardApiService } from '../../../dashboard/data/dashboard-api.service';
import { CategorySummary, ProductSummary } from '../../../dashboard/models/dashboard.models';
import { DocumentsApiService } from '../../data/documents-api.service';
import { AnalyzedDocument, WarrantyFromDocumentResult } from '../../models/document.models';

interface DetectedProductCandidate {
  id: string;
  sourceDescription: string;
  name: string;
  brand: string;
  model: string;
  categoryId: string;
  shouldAdd: boolean;
  createdProductId: string | null;
  shouldCreateWarranty: boolean;
  warrantyMonths: number;
  warrantyCreatedId: string | null;
}

@Component({
  selector: 'app-documents-page',
  imports: [
    DatePipe,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    FormsModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    ReactiveFormsModule
  ],
  templateUrl: './documents-page.component.html',
  styleUrl: './documents-page.component.scss'
})
export class DocumentsPageComponent {
  private readonly authViewModel = inject(AuthViewModel);
  private readonly dashboardApi = inject(DashboardApiService);
  private readonly documentsApi = inject(DocumentsApiService);
  private readonly uiFeedback = inject(UiFeedbackService);
  private readonly destroyRef = inject(DestroyRef);

  readonly loading = signal(true);
  readonly uploading = signal(false);
  readonly creatingProducts = signal(false);
  readonly creatingWarranties = signal(false);
  readonly products = signal<ProductSummary[]>([]);
  readonly categories = signal<CategorySummary[]>([]);
  readonly documents = signal<AnalyzedDocument[]>([]);
  readonly selectedDocumentId = signal<string | null>(null);
  readonly detectedCandidates = signal<DetectedProductCandidate[]>([]);
  readonly createdWarranties = signal<WarrantyFromDocumentResult[]>([]);
  readonly workflowMessage = signal<string | null>(null);

  readonly selectedDocument = computed(
    () => this.documents().find((document) => document.documentId === this.selectedDocumentId()) ?? null
  );
  readonly currentUserId = computed(() => this.authViewModel.user()?.userId || this.authViewModel.user()?.id || '');
  readonly suggestedWarrantyMonths = computed(() => this.selectedDocument()?.warrantyDurationMonths ?? 24);
  readonly productsReadyToCreate = computed(() =>
    this.detectedCandidates().filter((candidate) => candidate.shouldAdd && !candidate.createdProductId)
  );
  readonly productsReadyForWarranty = computed(() =>
    this.detectedCandidates().filter((candidate) => candidate.createdProductId && candidate.shouldCreateWarranty && !candidate.warrantyCreatedId)
  );

  constructor() {
    this.loadData();
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];

    if (!file) {
      return;
    }

    const userId = this.currentUserId();
    if (!userId) {
      this.uiFeedback.error('Trebuie sa fii autentificat ca sa incarci documente.');
      input.value = '';
      return;
    }

    this.uploading.set(true);
    this.documentsApi
      .analyzeDocument(file, userId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (document) => {
          this.documents.update((items) => [document, ...items.filter((item) => item.documentId !== document.documentId)]);
          this.selectDocument(document.documentId);
          this.uiFeedback.success('Documentul a fost analizat.');
        },
        error: () => {
          this.uiFeedback.error('Nu am putut analiza documentul.');
        },
        complete: () => {
          this.uploading.set(false);
          input.value = '';
        }
      });
  }

  selectDocument(documentId: string) {
    this.selectedDocumentId.set(documentId);
    this.createdWarranties.set([]);
    this.workflowMessage.set(null);
    const document = this.documents().find((item) => item.documentId === documentId) ?? null;
    this.detectedCandidates.set(document ? this.buildCandidates(document, this.categories()) : []);
  }

  updateCandidate(candidateId: string, updates: Partial<DetectedProductCandidate>) {
    this.detectedCandidates.update((items) =>
      items.map((candidate) => (candidate.id === candidateId ? { ...candidate, ...updates } : candidate))
    );
  }

  addSelectedProducts() {
    if (!this.currentUserId()) {
      this.workflowMessage.set('Trebuie sa fii autentificat ca sa adaugi produse.');
      return;
    }

    const candidates = this.productsReadyToCreate();
    if (candidates.length === 0) {
      this.workflowMessage.set('Nu exista produse selectate pentru adaugare sau toate au fost deja adaugate.');
      return;
    }

    const missingCategory = candidates.find((candidate) => !candidate.categoryId);
    if (missingCategory) {
      this.workflowMessage.set(`Alege o categorie pentru produsul "${missingCategory.name}".`);
      return;
    }

    this.creatingProducts.set(true);
    this.workflowMessage.set(null);

    const requests = candidates.map((candidate) =>
      this.dashboardApi.createProduct({
        name: candidate.name,
        brand: candidate.brand,
        model: candidate.model,
        categoryId: candidate.categoryId,
        userId: this.currentUserId()
      })
    );

    forkJoin(requests)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (createdProducts) => {
          const queue = [...candidates];
          this.products.update((items) => [...createdProducts, ...items]);
          this.detectedCandidates.update((items) =>
            items.map((candidate) => {
              const created = queue.find((item) => item.id === candidate.id);
              if (!created) {
                return candidate;
              }

              const product = createdProducts[queue.findIndex((item) => item.id === candidate.id)];
              return {
                ...candidate,
                createdProductId: product.productId
              };
            })
          );
          this.workflowMessage.set('Produsele selectate au fost adaugate. Acum poti decide pentru care creezi garantie.');
          this.uiFeedback.success('Produsele selectate au fost adaugate in catalog.');
        },
        error: () => {
          this.uiFeedback.error('Nu am putut adauga toate produsele detectate.');
        },
        complete: () => this.creatingProducts.set(false)
      });
  }

  createWarranties() {
    const selectedDocumentId = this.selectedDocumentId();
    const userId = this.currentUserId();
    const candidates = this.productsReadyForWarranty();

    if (!selectedDocumentId || !userId) {
      this.workflowMessage.set('Lipseste utilizatorul curent sau documentul selectat.');
      return;
    }

    if (candidates.length === 0) {
      this.workflowMessage.set('Nu exista produse pregatite pentru garantie.');
      return;
    }

    this.creatingWarranties.set(true);
    this.workflowMessage.set(null);

    const requests = candidates.map((candidate) =>
      this.documentsApi.createWarrantyFromAnalyzedDocument(selectedDocumentId, {
        userId,
        productId: candidate.createdProductId!,
        defaultDurationMonths: candidate.warrantyMonths
      })
    );

    forkJoin(requests)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (results) => {
          this.createdWarranties.update((items) => [...results, ...items]);
          this.detectedCandidates.update((items) =>
            items.map((candidate) => {
              const index = candidates.findIndex((item) => item.id === candidate.id);
              if (index === -1) {
                return candidate;
              }

              return {
                ...candidate,
                warrantyCreatedId: results[index].warranty.warrantyId
              };
            })
          );
          this.workflowMessage.set('Garantiile au fost create pentru produsele selectate.');
          this.uiFeedback.success('Garantiile au fost create din document.');
        },
        error: () => {
          this.uiFeedback.error('Nu am putut crea garantiile pentru produsele selectate.');
        },
        complete: () => this.creatingWarranties.set(false)
      });
  }

  productLabel(productId: string) {
    const product = this.products().find((item) => item.productId === productId);
    return product ? `${product.brand} ${product.name} ${product.model}`.trim() : 'Produs necunoscut';
  }

  private loadData() {
    this.loading.set(true);

    forkJoin({
      categories: this.dashboardApi.getCategories(this.currentUserId()),
      products: this.dashboardApi.getProductsCatalog(this.currentUserId()),
      documents: this.documentsApi.getDocuments(this.currentUserId())
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: ({ categories, products, documents }) => {
          const sortedDocuments = [...documents].sort(
            (left, right) => new Date(right.uploadedAt).getTime() - new Date(left.uploadedAt).getTime()
          );

          this.categories.set(categories);
          this.products.set(products);
          this.documents.set(sortedDocuments);

          if (sortedDocuments.length > 0) {
            this.selectDocument(sortedDocuments[0].documentId);
          }

          this.loading.set(false);
        },
        error: () => {
          this.loading.set(false);
          this.uiFeedback.error('Nu am putut incarca pagina de documente.');
        }
      });
  }

  private buildCandidates(document: AnalyzedDocument, categories: CategorySummary[]) {
    const categoryId = categories[0]?.categoryId ?? '';
    const sourceDescriptions = Array.from(
      new Set(
        document.lineItems
          .map((item) => item.description?.trim())
          .filter((description): description is string => Boolean(description && this.isLikelyProduct(description)))
      )
    );

    const normalizedDescriptions =
      sourceDescriptions.length > 0 ? sourceDescriptions : [this.fallbackDescription(document)].filter(Boolean);

    return normalizedDescriptions.map((description, index) => {
      const parsed = this.parseProductDescription(description);
      const months = document.warrantyDurationMonths ?? 24;

      return {
        id: `${document.documentId}-${index}`,
        sourceDescription: description,
        name: parsed.name,
        brand: parsed.brand,
        model: parsed.model,
        categoryId,
        shouldAdd: true,
        createdProductId: null,
        shouldCreateWarranty: true,
        warrantyMonths: months,
        warrantyCreatedId: null
      } satisfies DetectedProductCandidate;
    });
  }

  private isLikelyProduct(description: string) {
    const value = description.toLowerCase();
    return !['tva', 'tax', 'discount', 'transport', 'shipping', 'subtotal', 'total'].some((token) => value.includes(token));
  }

  private fallbackDescription(document: AnalyzedDocument) {
    return (
      document.lineItems.find((item) => item.description?.trim())?.description?.trim() ||
      document.originalFileName.replace(/\.[^/.]+$/, '') ||
      document.merchantName ||
      'Produs detectat'
    );
  }

  private parseProductDescription(description: string) {
    const cleaned = description.replace(/\s+/g, ' ').trim();
    const tokens = cleaned.split(' ').filter(Boolean);
    const brand = tokens[0] ?? 'Generic';
    const model = tokens.length > 1 ? tokens.slice(-2).join(' ') : 'Standard';
    const name = cleaned.length > 2 ? cleaned : `${brand} produs`;

    return { name, brand, model };
  }
}
