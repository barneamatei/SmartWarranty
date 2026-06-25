namespace DocumentAnalysis.Domain.DTOs;

public class AnalyzedDocumentResponseDto
{
    public Guid DocumentId { get; set; }
    public Guid? UserId { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string ExtractedText { get; set; } = string.Empty;
    public string? MerchantName { get; set; }
    public string? DocumentNumber { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string? CustomerName { get; set; }
    public decimal? TotalAmount { get; set; }
    public decimal? Subtotal { get; set; }
    public decimal? TaxAmount { get; set; }
    public string? Currency { get; set; }
    public int? WarrantyDurationMonths { get; set; }
    public List<ExtractedLineItemDto> LineItems { get; set; } = [];
    public bool UsedOcr { get; set; }
    public string? ErrorMessage { get; set; }
}
