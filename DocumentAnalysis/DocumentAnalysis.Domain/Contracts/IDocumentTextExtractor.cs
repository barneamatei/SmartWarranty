using DocumentAnalysis.Domain.DTOs;

namespace DocumentAnalysis.Domain.Contracts;

public interface IDocumentTextExtractor
{
    bool CanHandle(string contentType, string fileName);

    Task<ExtractedDocumentData> ExtractAsync(string filePath, string contentType, string fileName, CancellationToken cancellationToken = default);
}
