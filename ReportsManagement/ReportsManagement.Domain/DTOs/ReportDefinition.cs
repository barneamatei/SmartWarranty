namespace ReportsManagement.Domain.DTOs;

public sealed class ReportDefinition
{
    public required string ReportType { get; init; }
    public required string Title { get; init; }
    public string? Subtitle { get; init; }
    public required string FileNameStem { get; init; }
    public required DateTime GeneratedAtUtc { get; init; }
    public required IReadOnlyList<string> Columns { get; init; }
    public required IReadOnlyList<IReadOnlyList<string>> Rows { get; init; }
    public IReadOnlyDictionary<string, string> Summary { get; init; } = new Dictionary<string, string>();
}
