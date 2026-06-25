using System.Globalization;
using ReportsManagement.Domain.Contracts;
using ReportsManagement.Domain.DTOs;

namespace ReportsManagement.Infrastructure.Export;

public class PdfReportExporter : IReportExporter
{
    private const double PageWidth = 595;
    private const double PageHeight = 842;
    private const double Margin = 40;
    private const double ContentWidth = PageWidth - (Margin * 2);

    public string Format => "pdf";
    public string ContentType => "application/pdf";

    public byte[] Export(ReportDefinition report)
        => SimplePdfWriter.CreateDocument(BuildPages(report));

    private static IReadOnlyList<PdfPageContent> BuildPages(ReportDefinition report)
    {
        var pages = new List<PdfPageContent> { BuildOverviewPage(report) };
        pages.AddRange(BuildTablePages(report));
        return pages;
    }

    private static PdfPageContent BuildOverviewPage(ReportDefinition report)
    {
        var page = NewPage();
        DrawHeader(page, report);
        DrawSummaryCards(page, report);
        DrawStatusChart(page, report);
        DrawExpiryCalendar(page, report);
        DrawFooter(page, 1);
        return page;
    }

    private static PdfPageContent NewPage()
    {
        var page = new PdfPageContent();
        page.FillRect(0, 0, PageWidth, PageHeight, "F8FAFC");
        return page;
    }

    private static void DrawHeader(PdfPageContent page, ReportDefinition report)
    {
        page.FillRect(0, PageHeight - 118, PageWidth, 118, "16213E");
        page.FillRect(0, PageHeight - 118, 8, 118, "18B6A7");
        page.Text("SmartWarranty Reports", Margin, 794, 10, false, "B6C7E5");
        page.Text(report.Title, Margin, 766, 24, true, "FFFFFF");
        page.Text(report.Subtitle ?? "Warranty analytics export", Margin, 744, 10, false, "DDE7F6");
        page.Text($"Generated UTC: {report.GeneratedAtUtc:yyyy-MM-dd HH:mm}", 392, 794, 9, false, "DDE7F6");
        page.Text($"{report.Rows.Count} records", 392, 774, 16, true, "FFFFFF");
    }

    private static void DrawSummaryCards(PdfPageContent page, ReportDefinition report)
    {
        var summary = report.Summary.Take(4).ToList();
        if (summary.Count == 0)
        {
            summary.Add(new KeyValuePair<string, string>("Records", report.Rows.Count.ToString(CultureInfo.InvariantCulture)));
        }

        var cardWidth = (ContentWidth - 24) / 4;
        for (var index = 0; index < 4; index++)
        {
            var entry = index < summary.Count
                ? summary[index]
                : new KeyValuePair<string, string>("", "");
            var x = Margin + (index * (cardWidth + 8));

            page.FillRect(x, 656, cardWidth, 64, "FFFFFF");
            page.StrokeRect(x, 656, cardWidth, 64, "E2E8F0");
            page.Text(Trim(entry.Key, 22), x + 12, 698, 8, false, "64748B");
            page.Text(Trim(entry.Value, 14), x + 12, 674, 20, true, index == 0 ? "0F766E" : "1E293B");
        }
    }

    private static void DrawStatusChart(PdfPageContent page, ReportDefinition report)
    {
        page.Text("Warranty Status", Margin, 618, 14, true, "16213E");
        page.Text("Distribution across exported warranties", Margin, 602, 8, false, "64748B");

        var statusIndex = IndexOf(report.Columns, "Warranty Status");
        var statuses = statusIndex < 0
            ? new Dictionary<string, int>()
            : report.Rows
                .Where(row => row.Count > statusIndex)
                .GroupBy(row => string.IsNullOrWhiteSpace(row[statusIndex]) ? "Unknown" : row[statusIndex])
                .OrderByDescending(group => group.Count())
                .Take(5)
                .ToDictionary(group => group.Key, group => group.Count());

        if (statuses.Count == 0)
        {
            page.Text("No status data available for this report.", Margin, 566, 9, false, "64748B");
            return;
        }

        var max = statuses.Values.Max();
        var y = 574d;
        foreach (var status in statuses)
        {
            var barWidth = max == 0 ? 0 : (status.Value / (double)max) * 285;
            page.Text(Trim(status.Key, 18), Margin, y + 4, 8, true, "334155");
            page.FillRect(150, y, 300, 12, "E2E8F0");
            page.FillRect(150, y, Math.Max(5, barWidth), 12, StatusColor(status.Key));
            page.Text(status.Value.ToString(CultureInfo.InvariantCulture), 462, y + 3, 8, true, "334155");
            y -= 24;
        }
    }

    private static void DrawExpiryCalendar(PdfPageContent page, ReportDefinition report)
    {
        var expiryIndex = IndexOf(report.Columns, "Expiry Date");
        if (expiryIndex < 0)
            return;

        page.Text(report.ReportType == "expiring-warranties" ? "Expiry Calendar" : "Expiry Timeline", Margin, 430, 14, true, "16213E");
        page.Text("Grouped by month, so the presentation has an instant visual story.", Margin, 414, 8, false, "64748B");

        var productIndex = IndexOf(report.Columns, "Product");
        var items = report.Rows
            .Where(row => row.Count > expiryIndex && DateTime.TryParse(row[expiryIndex], CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            .Select(row =>
            {
                var date = DateTime.Parse(row[expiryIndex], CultureInfo.InvariantCulture);
                var product = productIndex >= 0 && row.Count > productIndex ? row[productIndex] : "Warranty";
                return new ExpiryItem(date, product);
            })
            .GroupBy(item => new DateTime(item.Date.Year, item.Date.Month, 1))
            .OrderBy(group => group.Key)
            .Take(6)
            .ToList();

        if (items.Count == 0)
        {
            page.Text("No expiry dates available for this report.", Margin, 378, 9, false, "64748B");
            return;
        }

        var boxWidth = (ContentWidth - 20) / 3;
        for (var index = 0; index < items.Count; index++)
        {
            var group = items[index];
            var x = Margin + ((index % 3) * (boxWidth + 10));
            var y = 314 - ((index / 3) * 92);

            page.FillRect(x, y, boxWidth, 74, "FFFFFF");
            page.StrokeRect(x, y, boxWidth, 74, "E2E8F0");
            page.FillRect(x, y + 58, boxWidth, 16, "E0F2FE");
            page.Text(group.Key.ToString("MMM yyyy", CultureInfo.InvariantCulture), x + 10, y + 62, 8, true, "075985");
            page.Text(group.Count().ToString(CultureInfo.InvariantCulture), x + boxWidth - 28, y + 37, 22, true, "0F766E");
            page.Text("expiring", x + boxWidth - 52, y + 24, 7, false, "64748B");

            var sampleY = y + 42;
            foreach (var item in group.OrderBy(item => item.Date).Take(2))
            {
                page.Text($"{item.Date:dd MMM} - {Trim(item.Product, 22)}", x + 10, sampleY, 7, false, "334155");
                sampleY -= 12;
            }
        }
    }

    private static IEnumerable<PdfPageContent> BuildTablePages(ReportDefinition report)
    {
        var selectedColumns = SelectColumns(report);
        const int rowsPerPage = 22;
        var pageNumber = 2;

        for (var start = 0; start < report.Rows.Count || start == 0; start += rowsPerPage)
        {
            var page = NewPage();
            DrawCompactTitle(page, report, pageNumber);
            DrawTable(page, report, selectedColumns, report.Rows.Skip(start).Take(rowsPerPage).ToList());
            DrawFooter(page, pageNumber);
            yield return page;
            pageNumber++;

            if (report.Rows.Count == 0)
                break;
        }
    }

    private static void DrawCompactTitle(PdfPageContent page, ReportDefinition report, int pageNumber)
    {
        page.FillRect(0, PageHeight - 72, PageWidth, 72, "16213E");
        page.Text(report.Title, Margin, 790, 16, true, "FFFFFF");
        page.Text($"Detail rows - page {pageNumber}", Margin, 772, 9, false, "DDE7F6");
    }

    private static void DrawTable(
        PdfPageContent page,
        ReportDefinition report,
        IReadOnlyList<ColumnSpec> columns,
        IReadOnlyList<IReadOnlyList<string>> rows)
    {
        var x = Margin;
        var y = 724d;
        var rowHeight = 27d;

        page.FillRect(Margin, y, ContentWidth, 22, "E2E8F0");
        foreach (var column in columns)
        {
            page.Text(column.Title, x + 6, y + 7, 7, true, "334155");
            x += column.Width;
        }

        y -= rowHeight;
        for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        {
            var row = rows[rowIndex];
            page.FillRect(Margin, y - 2, ContentWidth, rowHeight, rowIndex % 2 == 0 ? "FFFFFF" : "F1F5F9");

            x = Margin;
            foreach (var column in columns)
            {
                var value = column.Index >= 0 && row.Count > column.Index ? row[column.Index] : "-";
                if (column.Title.Contains("Status", StringComparison.OrdinalIgnoreCase))
                {
                    page.FillRect(x + 5, y + 6, Math.Min(column.Width - 10, 58), 12, StatusSoftColor(value));
                    page.Text(Trim(value, 12), x + 9, y + 9, 6.5, true, StatusColor(value));
                }
                else
                {
                    page.Text(Trim(value, column.MaxChars), x + 6, y + 9, 7, false, "334155");
                }

                x += column.Width;
            }

            page.Line(Margin, y - 2, Margin + ContentWidth, y - 2, "E2E8F0", 0.5);
            y -= rowHeight;
        }

        if (rows.Count == 0)
            page.Text("No rows for this report.", Margin + 6, y + 9, 9, false, "64748B");
    }

    private static IReadOnlyList<ColumnSpec> SelectColumns(ReportDefinition report)
    {
        if (report.ReportType == "expiring-warranties")
        {
            return
            [
                Column(report, "Customer", 88, 18),
                Column(report, "Product", 156, 32),
                Column(report, "Warranty Status", 74, 14),
                Column(report, "Expiry Date", 72, 12),
                Column(report, "Days Remaining", 58, 8),
                Column(report, "Plan", 67, 14)
            ];
        }

        return
        [
            Column(report, "Customer", 82, 17),
            Column(report, "Product", 146, 30),
            Column(report, "Warranty Status", 72, 14),
            Column(report, "Purchase Date", 70, 12),
            Column(report, "Expiry Date", 70, 12),
            Column(report, "Open Claims", 75, 8)
        ];
    }

    private static ColumnSpec Column(ReportDefinition report, string title, double width, int maxChars)
        => new(title, IndexOf(report.Columns, title), width, maxChars);

    private static void DrawFooter(PdfPageContent page, int pageNumber)
    {
        page.Line(Margin, 36, Margin + ContentWidth, 36, "CBD5E1");
        page.Text("SmartWarranty", Margin, 20, 8, true, "64748B");
        page.Text($"Page {pageNumber}", 505, 20, 8, false, "64748B");
    }

    private static int IndexOf(IReadOnlyList<string> columns, string name)
    {
        for (var index = 0; index < columns.Count; index++)
        {
            if (string.Equals(columns[index], name, StringComparison.OrdinalIgnoreCase))
                return index;
        }

        return -1;
    }

    private static string Trim(string value, int maxLength)
        => value.Length <= maxLength ? value : value[..Math.Max(0, maxLength - 3)] + "...";

    private static string StatusColor(string status)
    {
        var normalized = status.ToLowerInvariant();
        if (normalized.Contains("active") || normalized.Contains("valid"))
            return "0F766E";
        if (normalized.Contains("expir"))
            return "DC2626";
        if (normalized.Contains("pending") || normalized.Contains("claim"))
            return "B45309";
        return "2563EB";
    }

    private static string StatusSoftColor(string status)
    {
        var normalized = status.ToLowerInvariant();
        if (normalized.Contains("active") || normalized.Contains("valid"))
            return "CCFBF1";
        if (normalized.Contains("expir"))
            return "FEE2E2";
        if (normalized.Contains("pending") || normalized.Contains("claim"))
            return "FEF3C7";
        return "DBEAFE";
    }

    private sealed record ColumnSpec(string Title, int Index, double Width, int MaxChars);

    private sealed record ExpiryItem(DateTime Date, string Product);
}
