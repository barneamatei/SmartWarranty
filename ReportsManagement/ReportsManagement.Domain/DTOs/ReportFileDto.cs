namespace ReportsManagement.Domain.DTOs;

public sealed class ReportFileDto
{
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
    public required byte[] Content { get; init; }
}
