using DocumentAnalysis.Domain.Contracts;
using DocumentAnalysis.Domain.DTOs;
using DocumentAnalysis.Infrastructure.Parsing;

namespace DocumentAnalysis.Infrastructure.Tasks;

public class DocumentMetadataExtractor : IDocumentMetadataExtractor
{
    public ExtractedDocumentData ExtractFromText(string text, bool usedOcr)
    {
        return DocumentHeuristicsParser.Parse(text, usedOcr);
    }
}
