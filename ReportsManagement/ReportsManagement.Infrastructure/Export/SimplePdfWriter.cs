using System.Globalization;
using System.Text;

namespace ReportsManagement.Infrastructure.Export;

internal sealed class PdfPageContent
{
    private readonly StringBuilder _builder = new();

    public void Text(string value, double x, double y, double size = 10, bool bold = false, string color = "1F2937")
    {
        var (r, g, b) = Color(color);
        _builder
            .Append("BT\n")
            .Append('/').Append(bold ? "F2" : "F1").Append(' ').Append(Number(size)).Append(" Tf\n")
            .Append(Number(r)).Append(' ').Append(Number(g)).Append(' ').Append(Number(b)).Append(" rg\n")
            .Append(Number(x)).Append(' ').Append(Number(y)).Append(" Td\n")
            .Append('(').Append(Escape(Clean(value))).Append(") Tj\n")
            .Append("ET\n");
    }

    public void FillRect(double x, double y, double width, double height, string color)
    {
        var (r, g, b) = Color(color);
        _builder
            .Append("q\n")
            .Append(Number(r)).Append(' ').Append(Number(g)).Append(' ').Append(Number(b)).Append(" rg\n")
            .Append(Number(x)).Append(' ').Append(Number(y)).Append(' ').Append(Number(width)).Append(' ').Append(Number(height)).Append(" re f\n")
            .Append("Q\n");
    }

    public void StrokeRect(double x, double y, double width, double height, string color, double lineWidth = 1)
    {
        var (r, g, b) = Color(color);
        _builder
            .Append("q\n")
            .Append(Number(lineWidth)).Append(" w\n")
            .Append(Number(r)).Append(' ').Append(Number(g)).Append(' ').Append(Number(b)).Append(" RG\n")
            .Append(Number(x)).Append(' ').Append(Number(y)).Append(' ').Append(Number(width)).Append(' ').Append(Number(height)).Append(" re S\n")
            .Append("Q\n");
    }

    public void Line(double x1, double y1, double x2, double y2, string color, double lineWidth = 1)
    {
        var (r, g, b) = Color(color);
        _builder
            .Append("q\n")
            .Append(Number(lineWidth)).Append(" w\n")
            .Append(Number(r)).Append(' ').Append(Number(g)).Append(' ').Append(Number(b)).Append(" RG\n")
            .Append(Number(x1)).Append(' ').Append(Number(y1)).Append(" m ")
            .Append(Number(x2)).Append(' ').Append(Number(y2)).Append(" l S\n")
            .Append("Q\n");
    }

    internal string Build() => _builder.ToString();

    private static string Clean(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var character in value)
        {
            builder.Append(character is >= ' ' and <= '~' ? character : ' ');
        }

        return builder.ToString();
    }

    private static string Escape(string value)
        => value.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");

    private static (double R, double G, double B) Color(string hex)
    {
        hex = hex.TrimStart('#');
        var r = int.Parse(hex[..2], NumberStyles.HexNumber, CultureInfo.InvariantCulture) / 255d;
        var g = int.Parse(hex.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture) / 255d;
        var b = int.Parse(hex.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture) / 255d;
        return (r, g, b);
    }

    private static string Number(double value)
        => value.ToString("0.###", CultureInfo.InvariantCulture);
}

internal static class SimplePdfWriter
{
    public static byte[] CreateDocument(IReadOnlyList<IReadOnlyList<string>> pages)
    {
        var richPages = pages.Select(page =>
        {
            var content = new PdfPageContent();
            var y = 800d;
            foreach (var line in page)
            {
                content.Text(line, 40, y);
                y -= 14;
            }

            return content;
        }).ToList();

        return CreateDocument(richPages);
    }

    public static byte[] CreateDocument(IReadOnlyList<PdfPageContent> pages)
    {
        var pageObjectIds = new List<int>();
        const int catalogId = 1;
        const int pagesId = 2;
        const int fontId = 3;
        const int boldFontId = 4;
        var nextObjectId = 5;
        var objects = new SortedDictionary<int, byte[]>();

        foreach (var page in pages.Count == 0 ? [new PdfPageContent()] : pages)
        {
            var contentId = nextObjectId++;
            var pageId = nextObjectId++;
            pageObjectIds.Add(pageId);

            var contentStream = page.Build();
            var streamBytes = Encoding.ASCII.GetBytes(contentStream);
            objects[contentId] = Combine(
                Encoding.ASCII.GetBytes($"{contentId} 0 obj\n<< /Length {streamBytes.Length} >>\nstream\n"),
                streamBytes,
                Encoding.ASCII.GetBytes("\nendstream\nendobj\n"));

            var pageObject = $"{pageId} 0 obj\n<< /Type /Page /Parent {pagesId} 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 {fontId} 0 R /F2 {boldFontId} 0 R >> >> /Contents {contentId} 0 R >>\nendobj\n";
            objects[pageId] = Encoding.ASCII.GetBytes(pageObject);
        }

        var kids = string.Join(' ', pageObjectIds.Select(id => $"{id} 0 R"));
        objects[catalogId] = Encoding.ASCII.GetBytes($"{catalogId} 0 obj\n<< /Type /Catalog /Pages {pagesId} 0 R >>\nendobj\n");
        objects[pagesId] = Encoding.ASCII.GetBytes($"{pagesId} 0 obj\n<< /Type /Pages /Kids [{kids}] /Count {pageObjectIds.Count} >>\nendobj\n");
        objects[fontId] = Encoding.ASCII.GetBytes($"{fontId} 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>\nendobj\n");
        objects[boldFontId] = Encoding.ASCII.GetBytes($"{boldFontId} 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold >>\nendobj\n");

        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);
        writer.Write(Encoding.ASCII.GetBytes("%PDF-1.4\n"));

        var maxObjectId = objects.Keys.Max();
        var offsets = new long[maxObjectId + 1];
        foreach (var kvp in objects)
        {
            offsets[kvp.Key] = stream.Position;
            writer.Write(kvp.Value);
        }

        var xrefStart = stream.Position;
        writer.Write(Encoding.ASCII.GetBytes($"xref\n0 {maxObjectId + 1}\n"));
        writer.Write(Encoding.ASCII.GetBytes("0000000000 65535 f \n"));

        for (var objectId = 1; objectId <= maxObjectId; objectId++)
        {
            writer.Write(Encoding.ASCII.GetBytes($"{offsets[objectId].ToString("D10", CultureInfo.InvariantCulture)} 00000 n \n"));
        }

        writer.Write(Encoding.ASCII.GetBytes($"trailer\n<< /Size {maxObjectId + 1} /Root {catalogId} 0 R >>\nstartxref\n{xrefStart}\n%%EOF"));
        writer.Flush();
        return stream.ToArray();
    }

    private static byte[] Combine(params byte[][] chunks)
    {
        using var stream = new MemoryStream();
        foreach (var chunk in chunks)
        {
            stream.Write(chunk, 0, chunk.Length);
        }

        return stream.ToArray();
    }
}
