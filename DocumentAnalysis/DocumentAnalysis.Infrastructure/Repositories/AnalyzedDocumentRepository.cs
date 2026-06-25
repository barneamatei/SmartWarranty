using DocumentAnalysis.Domain.Contracts;
using DocumentAnalysis.Domain.Entities;
using DocumentAnalysis.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DocumentAnalysis.Infrastructure.Repositories;

public class AnalyzedDocumentRepository : IAnalyzedDocumentDao
{
    private readonly DocumentAnalysisDbContext _context;

    public AnalyzedDocumentRepository(DocumentAnalysisDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<AnalyzedDocument?> GetByIdAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        return await _context.Documents.FirstOrDefaultAsync(d => d.DocumentId == documentId, cancellationToken);
    }

    public async Task<IEnumerable<AnalyzedDocument>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AnalyzedDocument>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<AnalyzedDocument> AddAsync(AnalyzedDocument document, CancellationToken cancellationToken = default)
    {
        await _context.Documents.AddAsync(document, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return document;
    }

    public async Task<AnalyzedDocument> UpdateAsync(AnalyzedDocument document, CancellationToken cancellationToken = default)
    {
        _context.Documents.Update(document);
        await _context.SaveChangesAsync(cancellationToken);
        return document;
    }
}
