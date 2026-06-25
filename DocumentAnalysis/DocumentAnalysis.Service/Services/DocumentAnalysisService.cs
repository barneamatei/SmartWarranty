using DocumentAnalysis.Domain.Contracts;
using DocumentAnalysis.Domain.DTOs;
using DocumentAnalysis.Domain.Entities;
using DocumentAnalysis.Domain.Exceptions;
using Microsoft.AspNetCore.Http;

namespace DocumentAnalysis.Service.Services;

public class DocumentAnalysisService
{
    private readonly IAnalyzedDocumentDao _documentDao;
    private readonly IEnumerable<IDocumentTextExtractor> _extractors;
    private readonly IDocumentMetadataExtractor _metadataExtractor;

    public DocumentAnalysisService(
        IAnalyzedDocumentDao documentDao,
        IEnumerable<IDocumentTextExtractor> extractors,
        IDocumentMetadataExtractor metadataExtractor)
    {
        _documentDao = documentDao ?? throw new ArgumentNullException(nameof(documentDao));
        _extractors = extractors ?? throw new ArgumentNullException(nameof(extractors));
        _metadataExtractor = metadataExtractor ?? throw new ArgumentNullException(nameof(metadataExtractor));
    }

    public async Task<AnalyzedDocumentResponseDto> AnalyzeAsync(IFormFile file, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
            throw new DomainException("File is required.");

        var extractor = _extractors.FirstOrDefault(e => e.CanHandle(file.ContentType, file.FileName));
        if (extractor == null)
            throw new DomainException("Unsupported file type. Only PDF, PNG, JPG and JPEG are supported.");

        var document = new AnalyzedDocument(Guid.NewGuid(), file.FileName, file.ContentType, userId);
        document = await _documentDao.AddAsync(document, cancellationToken);

        var tempDirectory = Path.Combine(Path.GetTempPath(), "document-analysis");
        Directory.CreateDirectory(tempDirectory);
        var tempFilePath = Path.Combine(tempDirectory, $"{document.DocumentId}{Path.GetExtension(file.FileName)}");

        try
        {
            await using (var stream = File.Create(tempFilePath))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            var extracted = await extractor.ExtractAsync(tempFilePath, file.ContentType, file.FileName, cancellationToken);
            document.MarkProcessed(
                extracted.ExtractedText,
                extracted.DocumentType,
                extracted.MerchantName,
                extracted.DocumentNumber,
                extracted.IssueDate,
                extracted.TotalAmount,
                extracted.Currency,
                extracted.UsedOcr);

            var updated = await _documentDao.UpdateAsync(document, cancellationToken);
            return MapToResponse(updated);
        }
        catch (Exception ex) when (ex is not DomainException)
        {
            document.MarkFailed(ex.Message);
            await _documentDao.UpdateAsync(document, cancellationToken);
            throw new DomainException("Document analysis failed.", ex);
        }
        finally
        {
            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);
        }
    }

    public async Task<AnalyzedDocumentResponseDto?> GetByIdAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var document = await _documentDao.GetByIdAsync(documentId, cancellationToken);
        return document == null ? null : MapToResponse(document);
    }

    public async Task<IEnumerable<AnalyzedDocumentResponseDto>> GetAllAsync(Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var documents = userId.HasValue
            ? await _documentDao.GetByUserIdAsync(userId.Value, cancellationToken)
            : await _documentDao.GetAllAsync(cancellationToken);

        return documents.Select(MapToResponse);
    }

    public async Task<WarrantyDraftDto> CreateWarrantyDraftAsync(Guid documentId, CreateWarrantyDraftRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request.UserId == Guid.Empty)
            throw new DomainException("UserId is invalid.");
        if (request.ProductId == Guid.Empty)
            throw new DomainException("ProductId is invalid.");
        if (request.DefaultDurationMonths <= 0)
            throw new DomainException("DefaultDurationMonths must be greater than 0.");

        var document = await _documentDao.GetByIdAsync(documentId, cancellationToken);
        if (document == null)
            throw new DomainException($"Document with ID {documentId} not found.");
        if (document.Status != AnalysisStatus.Processed)
            throw new DomainException("Warranty draft can only be created from a processed document.");

        var derived = string.IsNullOrWhiteSpace(document.ExtractedText)
            ? new ExtractedDocumentData()
            : _metadataExtractor.ExtractFromText(document.ExtractedText, document.UsedOcr);

        var purchaseDate = document.IssueDate
            ?? derived.IssueDate
            ?? document.UploadedAt.Date;

        var durationMonths = derived.WarrantyDurationMonths ?? request.DefaultDurationMonths;

        return new WarrantyDraftDto
        {
            DocumentId = document.DocumentId,
            UserId = request.UserId,
            ProductId = request.ProductId,
            PurchaseDate = purchaseDate,
            DurationMonths = durationMonths,
            ProductDescription = derived.LineItems.FirstOrDefault()?.Description,
            MerchantName = document.MerchantName,
            DocumentNumber = document.DocumentNumber,
            TotalAmount = document.TotalAmount,
            Currency = document.Currency
        };
    }

    private AnalyzedDocumentResponseDto MapToResponse(AnalyzedDocument document)
    {
        var derived = string.IsNullOrWhiteSpace(document.ExtractedText)
            ? new ExtractedDocumentData()
            : _metadataExtractor.ExtractFromText(document.ExtractedText, document.UsedOcr);

        return new AnalyzedDocumentResponseDto
        {
            DocumentId = document.DocumentId,
            UserId = document.UserId,
            OriginalFileName = document.OriginalFileName,
            ContentType = document.ContentType,
            UploadedAt = document.UploadedAt,
            Status = document.Status.ToString(),
            DocumentType = document.DocumentType.ToString(),
            ExtractedText = document.ExtractedText,
            MerchantName = document.MerchantName,
            DocumentNumber = document.DocumentNumber,
            IssueDate = document.IssueDate,
            DueDate = derived.DueDate,
            CustomerName = derived.CustomerName,
            TotalAmount = document.TotalAmount,
            Subtotal = derived.Subtotal,
            TaxAmount = derived.TaxAmount,
            Currency = document.Currency,
            WarrantyDurationMonths = derived.WarrantyDurationMonths,
            LineItems = derived.LineItems,
            UsedOcr = document.UsedOcr,
            ErrorMessage = document.ErrorMessage
        };
    }
}
