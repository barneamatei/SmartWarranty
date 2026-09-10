using System.Text;
using DocumentAnalysis.Domain.Contracts;
using DocumentAnalysis.Domain.DTOs;
using DocumentAnalysis.Domain.Exceptions;
using DocumentAnalysis.Infrastructure.Parsing;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

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
            Environment.NewLine + Environment.NewLine,
            document.GetPages().Select(ExtractPageText));

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new DomainException("PDF appears to be scanned. For now, only PDF files with embedded text are supported.");
        }

        return Task.FromResult(DocumentHeuristicsParser.Parse(text, usedOcr: false));
    }

    private static string ExtractPageText(Page page)
    {
        var words = page.GetWords()
            .Where(word => !string.IsNullOrWhiteSpace(word.Text))
            .OrderByDescending(word => word.BoundingBox.Top)
            .ThenBy(word => word.BoundingBox.Left)
            .ToList();

        if (words.Count == 0)
            return page.Text;

        var lines = new List<List<Word>>();
        foreach (var word in words)
        {
            var line = lines.FirstOrDefault(existingLine => IsSameLine(existingLine[0], word));
            if (line == null)
            {
                lines.Add([word]);
                continue;
            }

            line.Add(word);
        }

        var builder = new StringBuilder();
        foreach (var line in lines)
        {
            var lineText = string.Join(
                ' ',
                line.OrderBy(word => word.BoundingBox.Left).Select(word => word.Text));

            if (!string.IsNullOrWhiteSpace(lineText))
                builder.AppendLine(lineText);
        }

        return builder.ToString().Trim();
    }

    private static bool IsSameLine(Word firstWord, Word candidate)
    {
        var firstMiddle = (firstWord.BoundingBox.Top + firstWord.BoundingBox.Bottom) / 2;
        var candidateMiddle = (candidate.BoundingBox.Top + candidate.BoundingBox.Bottom) / 2;
        var tolerance = Math.Max(firstWord.BoundingBox.Height, candidate.BoundingBox.Height) * 0.5;

        return Math.Abs(firstMiddle - candidateMiddle) <= tolerance;
    }
}
