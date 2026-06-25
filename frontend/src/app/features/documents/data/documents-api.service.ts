import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';

import { apiConfig } from '../../../core/config/api.config';
import { AnalyzedDocument, WarrantyDraft, WarrantyFromDocumentResult } from '../models/document.models';

@Injectable({ providedIn: 'root' })
export class DocumentsApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = apiConfig.gatewayBaseUrl;

  analyzeDocument(file: File, userId: string) {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('userId', userId);
    return this.http.post<AnalyzedDocument>(`${this.baseUrl}/documents/api/document/analyze`, formData);
  }

  getDocuments(userId: string) {
    return this.http.get<AnalyzedDocument[]>(`${this.baseUrl}/documents/api/document`, userId ? { params: { userId } } : {});
  }

  createWarrantyDraft(documentId: string, payload: { userId: string; productId: string; defaultDurationMonths: number }) {
    return this.http.post<WarrantyDraft>(`${this.baseUrl}/documents/api/document/${documentId}/warranty-draft`, payload);
  }

  createWarrantyFromAnalyzedDocument(
    documentId: string,
    payload: { userId: string; productId: string; defaultDurationMonths: number }
  ) {
    return this.http.post<WarrantyFromDocumentResult>(
      `${this.baseUrl}/warranties/api/warranty/from-analyzed-document/${documentId}`,
      payload
    );
  }
}
