using WarrantyManagement.Domain.DTOs;

namespace WarrantyManagement.Domain.Contracts;

public interface IDocumentAnalysisClient
{
    Task<DocumentWarrantyDraftDto> CreateWarrantyDraftAsync(Guid documentId, CreateWarrantyFromAnalyzedDocumentDto request, CancellationToken cancellationToken = default);
}
