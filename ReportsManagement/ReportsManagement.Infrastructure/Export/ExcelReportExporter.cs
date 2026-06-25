using System.IO.Compression;
using System.Security;
using System.Text;
using ReportsManagement.Domain.Contracts;
using ReportsManagement.Domain.DTOs;

namespace ReportsManagement.Infrastructure.Export;

public class ExcelReportExporter : IReportExporter
{
    public string Format => "xlsx";
    public string ContentType => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public byte[] Export(ReportDefinition report)
    {
        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            WriteEntry(archive, "[Content_Types].xml", BuildContentTypes());
            WriteEntry(archive, "_rels/.rels", BuildRootRels());
            WriteEntry(archive, "xl/workbook.xml", BuildWorkbook());
            WriteEntry(archive, "xl/_rels/workbook.xml.rels", BuildWorkbookRels());
            WriteEntry(archive, "xl/styles.xml", BuildStyles());
            WriteEntry(archive, "xl/worksheets/sheet1.xml", BuildWorksheet(report));
        }

        return memoryStream.ToArray();
    }

    private static void WriteEntry(ZipArchive archive, string entryName, string content)
    {
        var entry = archive.CreateEntry(entryName, CompressionLevel.Fastest);
        using var writer = new StreamWriter(entry.Open(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        writer.Write(content);
    }

    private static string BuildContentTypes() => """
<?xml version="1.0" encoding="UTF-8"?>
<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml" />
  <Default Extension="xml" ContentType="application/xml" />
  <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml" />
  <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml" />
  <Override PartName="/xl/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml" />
</Types>
""";

    private static string BuildRootRels() => """
<?xml version="1.0" encoding="UTF-8"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml" />
</Relationships>
""";

    private static string BuildWorkbook() => """
<?xml version="1.0" encoding="UTF-8"?>
<workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
  <sheets>
    <sheet name="Report" sheetId="1" r:id="rId1" />
  </sheets>
</workbook>
""";

    private static string BuildWorkbookRels() => """
<?xml version="1.0" encoding="UTF-8"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml" />
  <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml" />
</Relationships>
""";

    private static string BuildStyles() => """
<?xml version="1.0" encoding="UTF-8"?>
<styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
  <fonts count="2">
    <font><sz val="11" /><name val="Calibri" /></font>
    <font><b /><sz val="11" /><name val="Calibri" /></font>
  </fonts>
  <fills count="2">
    <fill><patternFill patternType="none" /></fill>
    <fill><patternFill patternType="gray125" /></fill>
  </fills>
  <borders count="1">
    <border><left /><right /><top /><bottom /><diagonal /></border>
  </borders>
  <cellStyleXfs count="1">
    <xf numFmtId="0" fontId="0" fillId="0" borderId="0" />
  </cellStyleXfs>
  <cellXfs count="2">
    <xf numFmtId="0" fontId="0" fillId="0" borderId="0" xfId="0" applyFont="1" />
    <xf numFmtId="0" fontId="1" fillId="0" borderId="0" xfId="0" applyFont="1" />
  </cellXfs>
  <cellStyles count="1">
    <cellStyle name="Normal" xfId="0" builtinId="0" />
  </cellStyles>
</styleSheet>
""";

    private static string BuildWorksheet(ReportDefinition report)
    {
        var rows = new List<IReadOnlyList<string>>
        {
            new List<string> { report.Title },
            new List<string> { report.Subtitle ?? string.Empty },
            new List<string> { $"Generated at (UTC): {report.GeneratedAtUtc:yyyy-MM-dd HH:mm:ss}" },
            new List<string>()
        };

        if (report.Summary.Count > 0)
        {
            rows.Add(new List<string> { "Summary" });
            rows.AddRange(report.Summary.Select(entry => (IReadOnlyList<string>)new List<string> { entry.Key, entry.Value }));
            rows.Add(new List<string>());
        }

        rows.Add(report.Columns);
        rows.AddRange(report.Rows);

        var maxColumnCount = Math.Max(1, rows.Max(row => row.Count));
        var columns = string.Concat(Enumerable.Range(1, maxColumnCount).Select(index => $"<col min=\"{index}\" max=\"{index}\" width=\"22\" customWidth=\"1\" />"));
        var rowXml = new StringBuilder();
        var headerRowIndex = report.Summary.Count > 0 ? report.Summary.Count + 6 : 5;

        for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        {
            var row = rows[rowIndex];
            rowXml.Append($"<row r=\"{rowIndex + 1}\">");
            for (var columnIndex = 0; columnIndex < row.Count; columnIndex++)
            {
                var currentRowNumber = rowIndex + 1;
                var styleIndex = currentRowNumber == 1 || currentRowNumber == headerRowIndex ? 1 : 0;
                var cellReference = GetColumnName(columnIndex + 1) + (rowIndex + 1);
                rowXml.Append($"<c r=\"{cellReference}\" t=\"inlineStr\" s=\"{styleIndex}\"><is><t>{Escape(row[columnIndex])}</t></is></c>");
            }
            rowXml.Append("</row>");
        }

        return $"""
<?xml version="1.0" encoding="UTF-8"?>
<worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
  <cols>{columns}</cols>
  <sheetData>{rowXml}</sheetData>
</worksheet>
""";
    }

    private static string GetColumnName(int index)
    {
        var name = string.Empty;
        while (index > 0)
        {
            index--;
            name = (char)('A' + index % 26) + name;
            index /= 26;
        }

        return name;
    }

    private static string Escape(string value)
        => SecurityElement.Escape(value) ?? string.Empty;
}
