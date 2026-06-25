namespace DocumentAnalysis.Domain.Entities;

public class AnalyzedDocument
{
    public Guid DocumentId { get; private set; }
    public Guid? UserId { get; private set; }
    public string OriginalFileName { get; private set; }
    public string ContentType { get; private set; }
    public DateTime UploadedAt { get; private set; }
    public string ExtractedText { get; private set; }
    public DocumentType DocumentType { get; private set; }
    public string? MerchantName { get; private set; }
    public string? DocumentNumber { get; private set; }
    public DateTime? IssueDate { get; private set; }
    public decimal? TotalAmount { get; private set; }
    public string? Currency { get; private set; }
    public bool UsedOcr { get; private set; }
    public AnalysisStatus Status { get; private set; }
    public string? ErrorMessage { get; private set; }

    protected AnalyzedDocument()
    {
        OriginalFileName = string.Empty;
        ContentType = string.Empty;
        ExtractedText = string.Empty;
    }

    public AnalyzedDocument(Guid documentId, string originalFileName, string contentType, Guid? userId = null)
    {
        DocumentId = documentId;
        UserId = userId;
        OriginalFileName = originalFileName;
        ContentType = contentType;
        UploadedAt = DateTime.UtcNow;
        ExtractedText = string.Empty;
        DocumentType = DocumentType.Unknown;
        Status = AnalysisStatus.Pending;
    }

    public void MarkProcessed(
        string extractedText,
        DocumentType documentType,
        string? merchantName,
        string? documentNumber,
        DateTime? issueDate,
        decimal? totalAmount,
        string? currency,
        bool usedOcr)
    {
        ExtractedText = extractedText;
        DocumentType = documentType;
        MerchantName = merchantName;
        DocumentNumber = documentNumber;
        IssueDate = issueDate;
        TotalAmount = totalAmount;
        Currency = currency;
        UsedOcr = usedOcr;
        Status = AnalysisStatus.Processed;
        ErrorMessage = null;
    }

    public void MarkFailed(string errorMessage)
    {
        Status = AnalysisStatus.Failed;
        ErrorMessage = errorMessage;
    }
}
