using DocumentAnalysis.Domain.Entities;

namespace DocumentAnalysis.Domain.Contracts;

public interface IAnalyzedDocumentDao
{
    Task<AnalyzedDocument?> GetByIdAsync(Guid documentId, CancellationToken cancellationToken = default);

    Task<IEnumerable<AnalyzedDocument>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<AnalyzedDocument>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<AnalyzedDocument> AddAsync(AnalyzedDocument document, CancellationToken cancellationToken = default);

    Task<AnalyzedDocument> UpdateAsync(AnalyzedDocument document, CancellationToken cancellationToken = default);
}
