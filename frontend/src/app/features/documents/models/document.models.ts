export interface ExtractedLineItem {
  description: string;
  quantity?: number | null;
  unitPrice?: number | null;
  totalPrice?: number | null;
}

export interface AnalyzedDocument {
  documentId: string;
  userId?: string | null;
  originalFileName: string;
  contentType: string;
  uploadedAt: string;
  status: string;
  documentType: string;
  extractedText: string;
  merchantName?: string | null;
  documentNumber?: string | null;
  issueDate?: string | null;
  dueDate?: string | null;
  customerName?: string | null;
  totalAmount?: number | null;
  subtotal?: number | null;
  taxAmount?: number | null;
  currency?: string | null;
  warrantyDurationMonths?: number | null;
  lineItems: ExtractedLineItem[];
  usedOcr: boolean;
  errorMessage?: string | null;
}

export interface WarrantyDraft {
  documentId: string;
  userId: string;
  productId: string;
  purchaseDate: string;
  durationMonths: number;
  productDescription?: string | null;
  merchantName?: string | null;
  documentNumber?: string | null;
  totalAmount?: number | null;
  currency?: string | null;
}

export interface WarrantyFromDocumentResult {
  documentId: string;
  merchantName?: string | null;
  documentNumber?: string | null;
  productDescription?: string | null;
  totalAmount?: number | null;
  currency?: string | null;
  warranty: {
    warrantyId: string;
    userId: string;
    productId: string;
    purchaseDate: string;
    expiryDate: string;
    durationMonths: number;
    status: string;
  };
}
