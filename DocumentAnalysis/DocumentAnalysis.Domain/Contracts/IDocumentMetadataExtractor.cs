using DocumentAnalysis.Domain.DTOs;

namespace DocumentAnalysis.Domain.Contracts;

public interface IDocumentMetadataExtractor
{
    ExtractedDocumentData ExtractFromText(string text, bool usedOcr);
}
