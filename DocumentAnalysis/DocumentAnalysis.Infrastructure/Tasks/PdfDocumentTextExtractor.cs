using DocumentAnalysis.Domain.Contracts;
using DocumentAnalysis.Domain.DTOs;
using DocumentAnalysis.Domain.Exceptions;
using DocumentAnalysis.Infrastructure.Parsing;
using UglyToad.PdfPig;

namespace DocumentAnalysis.Infrastructure.Tasks;

public class PdfDocumentTextExtractor : IDocumentTextExtractor
{
    public bool CanHandle(string contentType, string fileName)
    {
        return contentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase) ||
               Path.GetExtension(fileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase);
    }

    public Task<ExtractedDocumentData> ExtractAsync(string filePath, string contentType, string fileName, CancellationToken cancellationToken = default)
    {
        using var document = PdfDocument.Open(filePath);
        var text = string.Join(
            Environment.NewLine,
            document.GetPages().Select(page => page.Text));

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new DomainException("PDF appears to be scanned. For now, only PDF files with embedded text are supported.");
        }

        return Task.FromResult(DocumentHeuristicsParser.Parse(text, usedOcr: false));
    }
}
