using DocumentAnalysis.Domain.Entities;

namespace DocumentAnalysis.Domain.DTOs;

public class ExtractedDocumentData
{
    public string ExtractedText { get; set; } = string.Empty;
    public DocumentType DocumentType { get; set; }
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
}
