namespace ReportsManagement.Domain.DTOs;

public sealed class ReportPreviewDto
{
    public required string ReportType { get; init; }
    public required string Title { get; init; }
    public string? Subtitle { get; init; }
    public required DateTime GeneratedAtUtc { get; init; }
    public required int RecordCount { get; init; }
    public required IReadOnlyList<string> Columns { get; init; }
    public required IReadOnlyList<IReadOnlyDictionary<string, string>> Rows { get; init; }
    public IReadOnlyDictionary<string, string> Summary { get; init; } = new Dictionary<string, string>();
}
